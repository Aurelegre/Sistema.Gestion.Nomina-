using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Gestion.Nómina.DTOs.Empleados;
using Sistema.Gestion.Nómina.DTOs.Nominas;
using Sistema.Gestion.Nómina.DTOs.SolicitudesAusencia;
using Sistema.Gestion.Nómina.Entitys;
using Sistema.Gestion.Nómina.Models;
using Sistema.Gestion.Nómina.Services.Logs;
using Sistema.Gestion.Nómina.Services.Nomina;
using System.Data;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Sistema.Gestion.Nómina.Controllers
{
    [Authorize]
    [Controller]
    public class NominaController(SistemaGestionNominaContext context, ILogServices logger, INominaServices nominaServices) : Controller
    {
        [HttpGet]
        [Authorize(Policy = "Nominas.Listar")]
        public async Task<ActionResult> Index(GetNominasDTO request)
        {
            var session = logger.GetSessionData();
            try
            { 
               
                var query = await ConsultarNomina(request.fecha);
                
                if (query == null)
                {
                    TempData["Error"] = $"Error al consultar Nómina, comuniquese con soporte";
                    return RedirectToAction("Index", "Employees");
                }
                if (query.Count() == 0)
                {
                    TempData["Error"] = $"No existe Nómina en el mes seleccionado";
                    return RedirectToAction("Index", "Employees");
                }
                if (!string.IsNullOrEmpty(request.Nombre))
                {
                    query = query.Where(e => e.NombreEmpleado.Contains(request.Nombre)).ToList();
                }
                if (!string.IsNullOrEmpty(request.Depto))
                {
                    query = query.Where(e => e.Departamento == request.Depto).ToList();
                }
                if (!string.IsNullOrEmpty(request.Puesto))
                {
                    query = query.Where(e => e.Puesto == request.Puesto).ToList();
                }
                var totalItems = query.Count();
                var nomina = query
                                .Skip((request.page - 1) * request.pageSize)
                                .Take(request.pageSize);
                var paginatedResult = new PaginatedResult<GetNominaModel>
                {
                    Items = nomina,
                    CurrentPage = request.page,
                    PageSize = request.pageSize,
                    TotalItems = totalItems
                };
                string fechaletras = request.fecha.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES"));
                ViewBag.fechaletras = fechaletras;
                ViewBag.Nombre = request.Nombre;
                ViewBag.Departamento = request.Depto;
                ViewBag.Puesto = request.Puesto;
                ViewBag.Fecha = request.fecha;
                await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.Index", $"Se consultó la query del mes {request.fecha} de la empresa {session.company}", session.nombre);
                return View(paginatedResult);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.Index", $"ERROR al consultar nómina de la empresa {session.company}", ex.Message, ex.StackTrace);
                TempData["Error"] = $"Error al consultar Nómina, comuniquese con soporte";
                return RedirectToAction("Index", "Employees");
            }
        }
        [HttpGet]
        [Authorize(Policy = "Nominas.Ver")]
        public async Task<ActionResult>Details(int id)
        {
            var session = logger.GetSessionData();
            try
            {
                var nomina = await context.Nominas
                                        .Where(e => e.Id == id)
                                        .AsNoTracking()
                                        .Select(e => new GetNominaModel
                                        {
                                            Id = e.Id,
                                            NombreEmpleado = e.IdEmpleadoNavigation.Nombre + " " + e.IdEmpleadoNavigation.Apellidos,
                                            Departamento = e.IdEmpleadoNavigation.IdDepartamentoNavigation.Descripcion,
                                            Puesto = e.IdEmpleadoNavigation.IdPuestoNavigation.Descripcion,
                                            Sueldo = e.Sueldo,
                                            SueldoExtra = e.SueldoExtra,
                                            Comisiones = e.Comisiones,
                                            Bonificaciones = e.Bonificaciones,
                                            AguinaldoBono = e.AguinaldoBono,
                                            OtrosIngresos = e.OtrosIngresos,
                                            TotalDevengado = e.TotalDevengado,
                                            TotalDescuentos = e.TotalDescuentos,
                                            TotalLiquido = e.TotalLiquido,
                                            Igss = e.Igss,
                                            Isr = e.Isr,
                                            Prestamos = e.Prestamos,
                                            Creditos = e.Creditos,
                                            Anticipos = e.Anticipos,
                                            OtrosDesc = e.OtrosDesc
                                        })
                                        .FirstOrDefaultAsync();
                if(nomina == null)
                {
                    TempData["Error"] = "Error al obtener detalle de Empleado";
                    return RedirectToAction("Index", "Nomina");
                }
                await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.Details", $"Se consultaron detalles de la nomina del empleado con id: {id}, Nombre: {nomina.NombreEmpleado}", session.nombre);

                return Json(nomina);
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.Details", $"Error al consultar detalles de la nomina del empleado con id: {id}", ex.Message, ex.StackTrace);
                TempData["Error"] = "Error al consultar detalles de Empleado";
                return RedirectToAction("Index", "Nomina");
            }
        }
        [HttpGet]
        [Authorize(Policy = "Nominas.Generar")]
        public async Task<ActionResult> GenerateNomina()
        {
            var session = logger.GetSessionData();
            try
            {
                DateOnly fecha = DateOnly.FromDateTime(DateTime.Now.Date);
                //verificar que no exista nomina generada en el mes y año actual
                var exist = await context.Nominas.AnyAsync(e => e.Fecha.Month == fecha.Month && e.Fecha.Year == fecha.Year && e.IdEmpresa == session.company);
                if (exist)
                {
                    TempData["Error"] = $"La nómina de este mes ya fue generada";
                    return RedirectToAction("Index", "Employees");
                }
                //traer la información de los empleados activos de la empresa que no sea el admnistrador general
                var empleados = await context.Empleados
                                                    .Where(e => e.Activo == 1 && e.IdEmpresa == session.company && !e.Dpi.Equals("0") && !e.Apellidos.Equals("Administrador"))
                                                       .AsNoTracking()
                                                       .Select(e => new GetEmpleadosModel
                                                       {
                                                           Id = e.Id,
                                                           Nombres = e.Nombre + " " + e.Apellidos,
                                                           DPI = e.Dpi,
                                                           Sueldo = e.Sueldo,
                                                           Departamento = e.IdDepartamentoNavigation.Descripcion,
                                                           Puesto = e.IdPuestoNavigation.Descripcion,
                                                           FechaContratado = e.FechaContratado
                                                       })
                                                       .ToListAsync();
                var generarNomina = await GenerarNomina(empleados);

                //error al generar nómina
                if (!generarNomina)
                {
                    TempData["Error"] = $"Error generar Nómina, comuniquese con soporte";
                    return RedirectToAction("Index", "Employees");
                }

                await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.GenerateNomina", $"Se registró la generación de nómina de la empresa {session.company}", session.nombre);
                return RedirectToAction("Index", "Nomina", new GetNominasDTO { fecha = fecha });

            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.GenerateNomina", $"ERROR al generar nómina de la empresa {session.company}", ex.Message, ex.StackTrace);
                TempData["Error"] = $"Error generar Nómina, comuniquese con soporte";
                return RedirectToAction("Index", "Employees");
            }
        }
        [HttpPost]
        [Authorize(Policy = "Nominas.Exportar")]
        public async Task<ActionResult> ExportNomina(DateOnly fecha)
        {
            var session = logger.GetSessionData();
            try
            {
                var nomina = await ConsultarNomina(fecha);
                if (nomina == null)
                {
                    TempData["Error"] = $"Error al Exportar Nómina, comuniquese con soporte";
                    return RedirectToAction("Index", "Nomina", new GetNominasDTO { fecha = fecha });
                }
                if (nomina.Count() == 0)
                {
                    TempData["Error"] = $"No existe Nómina en el mes seleccionado";
                    return RedirectToAction("Index", "Nomina", new GetNominasDTO { fecha = fecha });
                }
                //traer el nombre de la empresa
                var empresa = await context.Empresas.Where(e => e.Id == session.company).AsNoTracking().Select(e => e.Nombre).FirstOrDefaultAsync();
                // Convertir la lista a DataTable
                DataTable dataTableNomina = ConvertirNominaADataTable(nomina);

                using (var libro = new XLWorkbook())
                {
                    // Añadir el DataTable directamente a la hoja de cálculo
                    var hoja = libro.Worksheets.Add(dataTableNomina, "Nómina");
                    hoja.ColumnsUsed().AdjustToContents();

                    // Aplicar formato de moneda de la columna E a la S, comenzando en la fila 2
                    var rango = hoja.Range("E2:S" + hoja.LastRowUsed().RowNumber());
                    rango.Style.NumberFormat.Format = "Q #,##0.00"; // Formato de moneda de Guatemala (Quetzal)
                    
                    // Aplicar formato Negrita (Bold) a las columnas K, R y S
                    var columnaK = hoja.Range("K2:K" + hoja.LastRowUsed().RowNumber());
                    var columnaR = hoja.Range("R2:R" + hoja.LastRowUsed().RowNumber());
                    var columnaS = hoja.Range("S2:S" + hoja.LastRowUsed().RowNumber());

                    columnaK.Style.Font.SetBold(true);
                    columnaR.Style.Font.SetBold(true);
                    columnaS.Style.Font.SetBold(true);

                    // Aplicar bordes a la última fila con registros
                    var ultimaFila = hoja.LastRowUsed();
                    ultimaFila.Style.Font.SetBold(true);

                    using (var memoria = new MemoryStream())
                    {
                        libro.SaveAs(memoria);
                        var nombreExcel = string.Concat(empresa, " Nomina ", fecha.ToString(), ".xlsx");

                        await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.ExportarNómina", $"Se exportó nómina de la empresa {session.company}, {empresa} del mes {fecha}", session.nombre);

                        return File(memoria.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreExcel);
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.ExportarNómina", $"ERROR al generar nómina de la empresa {session.company} del mes {fecha}", ex.Message, ex.StackTrace);
                TempData["Error"] = $"Error exportar Nómina, comuniquese con soporte";
                return RedirectToAction("Index", "Nomina", new GetNominasDTO { fecha = fecha });
            }

        }
        private DataTable ConvertirNominaADataTable(List<GetNominaModel> nomina)
        {
            DataTable dataTable = new DataTable();

            // Definir las columnas del DataTable
            dataTable.Columns.Add("No.", typeof(int));
            dataTable.Columns.Add("Nombre", typeof(string));
            dataTable.Columns.Add("Departamento", typeof(string));
            dataTable.Columns.Add("Puesto", typeof(string));
            dataTable.Columns.Add("     Sueldo     ", typeof(decimal));
            dataTable.Columns.Add("Sueldo Extraordinario", typeof(decimal));
            dataTable.Columns.Add("Comisiones", typeof(decimal));
            dataTable.Columns.Add("Bonificación", typeof(decimal));
            dataTable.Columns.Add("Aguinaldo / Bono14", typeof(decimal));
            dataTable.Columns.Add("Otros Ingresos", typeof(decimal));
            dataTable.Columns.Add("Total Devengado", typeof(decimal));
            dataTable.Columns.Add("       IGSS       ", typeof(decimal));
            dataTable.Columns.Add("      ISR       ", typeof(decimal));
            dataTable.Columns.Add("Préstamos", typeof(decimal));
            dataTable.Columns.Add("Créditos", typeof(decimal));
            dataTable.Columns.Add("Anticipos", typeof(decimal));
            dataTable.Columns.Add("Otros Descuentos", typeof(decimal));
            dataTable.Columns.Add("Total Descuentos", typeof(decimal));
            dataTable.Columns.Add("Total Liquido", typeof(decimal));
            dataTable.Columns.Add("Firma del Empleado", typeof(string));

            int counter = 1;
            // Rellenar las filas con los datos de la nómina
            foreach (var item in nomina)
            {
                var row = dataTable.NewRow();
                row["No."] = counter;
                row["Nombre"] = item.NombreEmpleado;
                row["Departamento"] = item.Departamento;
                row["Puesto"] = item.Puesto;
                row["     Sueldo     "] = item.Sueldo ?? 0;
                row["Sueldo Extraordinario"] = item.SueldoExtra ?? 0;
                row["Comisiones"] = item.Comisiones ?? 0;
                row["Bonificación"] = item.Bonificaciones ?? 0;
                row["Aguinaldo / Bono14"] = item.AguinaldoBono ?? 0;
                row["Otros Ingresos"] = item.OtrosIngresos ?? 0;
                row["Total Devengado"] = item.TotalDevengado ?? 0;
                row["       IGSS       "] = item.Igss ?? 0;
                row["      ISR       "] = item.Isr ?? 0;
                row["Préstamos"] = item.Prestamos ?? 0;
                row["Créditos"] = item.Creditos ?? 0;
                row["Anticipos"] = item.Anticipos ?? 0;
                row["Otros Descuentos"] = item.OtrosDesc ?? 0;
                row["Total Descuentos"] = item.TotalDescuentos ?? 0;
                row["Total Liquido"] = item.TotalLiquido ?? 0;
                row["Firma del Empleado"] = "";


                dataTable.Rows.Add(row);
                counter++;
            }
            var row2 = dataTable.NewRow();
            row2["Nombre"] = "TOTAL:";
            row2["     Sueldo     "] = nomina.Sum(e=>e.Sueldo);
            row2["Sueldo Extraordinario"] = nomina.Sum(e => e.SueldoExtra);
            row2["Comisiones"] = nomina.Sum(e => e.Comisiones);
            row2["Bonificación"] = nomina.Sum(e => e.Bonificaciones);
            row2["Aguinaldo / Bono14"] = nomina.Sum(e => e.AguinaldoBono);
            row2["Otros Ingresos"] = nomina.Sum(e => e.OtrosIngresos);
            row2["Total Devengado"] = nomina.Sum(e => e.TotalDevengado);
            row2["       IGSS       "] = nomina.Sum(e => e.Igss);
            row2["      ISR       "] = nomina.Sum(e => e.Isr);
            row2["Préstamos"] = nomina.Sum(e => e.Prestamos);
            row2["Créditos"] = nomina.Sum(e => e.Creditos);
            row2["Anticipos"] = nomina.Sum(e => e.Anticipos);
            row2["Otros Descuentos"] = nomina.Sum(e => e.OtrosDesc);
            row2["Total Descuentos"] = nomina.Sum(e => e.TotalDescuentos);
            row2["Total Liquido"] = nomina.Sum(e => e.TotalLiquido);
            dataTable.Rows.Add(row2);
            return dataTable;
        }
        private async Task<bool> GenerarNomina (List<GetEmpleadosModel> empleados)
        {
            var session = logger.GetSessionData();
            try
            {
                foreach (var empleado in empleados)
                {
                    var departamento = empleado.Departamento;
                    var puesto = empleado.Puesto;
                    var sueldo = empleado.Sueldo;
                    var id = empleado.Id;
                    var fechaContratado = empleado.FechaContratado;

                    #region Aumentos
                    //Aumentos y Salario
                    #region HorasExtra
                    //traer todas la horas extras del mes en curso del empleado
                    var sumHorasExtras = await context.Aumento
                                    .Where(e => e.IdEmpleado == id && e.IdTipo == 1 && e.Fecha.Month == DateTime.Now.Month && e.Fecha.Year == DateTime.Now.Year)
                                    .AsNoTracking()
                                    .SumAsync(e => e.Total);
                    //calcular comisión a pagar por horas extras
                    decimal? horasExtras = nominaServices.CalcularHorasExtras(sueldo, sumHorasExtras);
                    //traer todas las horas extras de dias festivos del mes en curso del empleado
                    var sumFetivos = await context.Aumento
                                    .Where(e => e.IdEmpleado == id && e.IdTipo == 2 && e.Fecha.Month == DateTime.Now.Month && e.Fecha.Year == DateTime.Now.Year)
                                    .AsNoTracking()
                                    .SumAsync(e => e.Total);
                    //calcular comisión a pagar por dias festivos
                    decimal? diasFestivos = nominaServices.CalcularComisionDiafestivo(sueldo, sumFetivos);
                    #endregion
                    //sueldo extraordinario
                    decimal? sueldoExtraordinario = horasExtras.Value + diasFestivos.Value;

                    #region Comisiones
                    //traer todas la comisiones por ventas del empleado del mes actual
                    var sumComisionesVenta = await context.Aumento
                                     .Where(e => e.IdEmpleado == id && e.IdTipo == 4 && e.Fecha.Month == DateTime.Now.Month && e.Fecha.Year == DateTime.Now.Year)
                                     .AsNoTracking()
                                     .SumAsync(e => e.Total);
                    //calcular porcentaje a dar de comisión por venta
                    decimal? ComisionVenta = nominaServices.CalcularComisionVenta(id, sumComisionesVenta);
                    //traer todas la comisiones por producción del empleado en el mes actual
                    var sumComsionesProd = await context.Aumento
                                     .Where(e => e.IdEmpleado == id && e.IdTipo == 5 && e.Fecha.Month == DateTime.Now.Month && e.Fecha.Year == DateTime.Now.Year)
                                     .AsNoTracking()
                                     .SumAsync(e => e.Total);
                    //calcular pocentaje a dar de comisión por producción
                    decimal? comisionProd = nominaServices.CalcularComisionProd(id, sumComsionesProd);
                    #endregion
                    //total de Comisiones
                    decimal? comisiones = comisionProd.Value + ComisionVenta.Value;

                    //total devengado sin aguinaldo o bono
                    decimal? totalDevengado = sueldo.Value + comisiones.Value + sueldoExtraordinario.Value;



                    #endregion

                    #region Descuentos
                    //traer los días de las ausencias deducibles
                    var diasAusencia = await context.Ausencias
                                .Where(e => e.IdEmpleado == id && e.Deducible == 1)
                                .AsNoTracking()
                                .SumAsync(e => e.TotalDias);
                    //calcular el deducible por ausencias
                    decimal? descuentoAusencia = nominaServices.DescuentoAusencia(sueldo, diasAusencia);

                    //traer los prestamos activos del empleado 
                    var prestamos = await context.Prestamos
                                .Where(e => e.IdEmpleado == id && e.Pagado == 1 && e.CuotasPendientes != 0 && e.IdTipo == 1)
                                .AsNoTracking()
                                .ToListAsync();
                    //calcular total a pagar por prestamos activos
                    decimal? aPagarPrestamo = await PagarCuotaPrestamo(prestamos);
                    //traer los creditos activos del empleado
                    var creditos = await context.Prestamos
                                .Where(e => e.IdEmpleado == id && e.Pagado == 1 && e.CuotasPendientes != 0 && e.IdTipo == 2)
                                .AsNoTracking()
                                .ToListAsync();
                    //calcular total a pagar por creditos activos
                    decimal? aPagarCredito = await PagarCuotaPrestamo(creditos);

                    //calcular cuota laboral del IGSS
                    decimal? igss = nominaServices.CuotaLaboralIGSS(totalDevengado);
                    //calcular ISR
                    decimal? isr = await ProyectarISR(id, totalDevengado, 250.00m);
                    //verificar si posee anticipos del salario del mes actual
                    var poseeAnticipo = await context.Aumento.AnyAsync(e => e.IdEmpleado == id && e.IdTipo == 3 && e.Fecha.Month == DateTime.Now.Month);
                    //inicializar variable con el deducible de anticipo
                    decimal? anticipo = 0.00m;
                    if (poseeAnticipo)
                    {
                        //calcular el anticipo
                        anticipo = nominaServices.CalcularAdelanto(sueldo);
                    }
                    #endregion

                    #region Aguinaldo y Bono 14
                    //inicializar variable de bono o aguinaldo
                    decimal? aguinaldoBono = 0;
                    //validar si estamos en junio al momento de generar la nómina para calcular BONO14
                    var fechaActual = DateTime.Now;
                    if (fechaActual.Month == 6)
                    {
                        aguinaldoBono = nominaServices.CalcularBono14(sueldo, fechaContratado, fechaActual);
                    }
                    else if (fechaActual.Month == 12)
                    {
                        aguinaldoBono = nominaServices.CalcularAguinaldo(sueldo, fechaContratado, fechaActual);
                    }

                    #endregion

                    //total devengado por empleado
                    totalDevengado += aguinaldoBono + 250.00m;

                    //total descuentos
                    decimal? totaldescuentos = descuentoAusencia + aPagarPrestamo + aPagarCredito + igss + isr + anticipo;

                    //total Liquido
                    decimal? totalLiquido = totalDevengado - totaldescuentos;

                    //crear el registro del empleado en la nómina
                    Nomina nomina = new Nomina
                    {
                        IdEmpleado = id,
                        Sueldo = sueldo,
                        SueldoExtra = sueldoExtraordinario,
                        Comisiones = comisiones,
                        Bonificaciones = 250.00m,
                        AguinaldoBono = aguinaldoBono,
                        OtrosIngresos = 0.00m,
                        TotalDevengado = totalDevengado,
                        Igss = igss,
                        Isr = isr,
                        Prestamos = aPagarPrestamo,
                        Creditos = aPagarCredito,
                        Anticipos = anticipo,
                        OtrosDesc = 0.00m,
                        TotalDescuentos = totaldescuentos,
                        TotalLiquido = totalLiquido,
                        Fecha = DateTime.Now.Date,
                        IdEmpresa = session.company
                    };
                    await context.Nominas.AddAsync(nomina);
                }
                await context.SaveChangesAsync();

                await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.GenerarNomina", $"Se generó la nómina del mes {DateTime.Now.Date}, de la empresa {session.company}", session.nombre);
                return true;
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.GenerarNomina", $"ERROR al generar la nómina del mes {DateTime.Now.Date}, de la empresa {session.company}", ex.Message, ex.StackTrace);
                return false;
            }
        }
        private async Task<List<GetNominaModel>> ConsultarNomina(DateOnly Fecha)
        {
            var session = logger.GetSessionData();
            try
            {
                var nomina = await context.Nominas
                                   .Where(e=> e.IdEmpresa == session.company && e.Fecha.Month == Fecha.Month && e.Fecha.Year == Fecha.Year)
                                   .AsNoTracking()
                                   .Select(e=> new GetNominaModel
                                   {
                                        Id = e.Id,
                                        NombreEmpleado = e.IdEmpleadoNavigation.Nombre + " " + e.IdEmpleadoNavigation.Apellidos,
                                        Departamento = e.IdEmpleadoNavigation.IdDepartamentoNavigation.Descripcion,
                                        Puesto = e.IdEmpleadoNavigation.IdPuestoNavigation.Descripcion,
                                        Sueldo = e.Sueldo,
                                        SueldoExtra = e.SueldoExtra,
                                        Comisiones = e.Comisiones,
                                        Bonificaciones = e.Bonificaciones,
                                        AguinaldoBono = e.AguinaldoBono,
                                        OtrosIngresos = e.OtrosIngresos,
                                        TotalDevengado = e.TotalDevengado,
                                        TotalDescuentos = e.TotalDescuentos,
                                        TotalLiquido = e.TotalLiquido,
                                        Igss = e.Igss,
                                        Isr = e.Isr,
                                        Prestamos = e.Prestamos,
                                        Creditos = e.Creditos,
                                        Anticipos = e.Anticipos,
                                        OtrosDesc = e.OtrosDesc
                                   })
                                   .ToListAsync();
                if(nomina == null)
                {
                    return null;
                }
                await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.ConsultarNomina", $"Se consultó la nómina correspondiente al mes de {Fecha}", session.nombre);
                return nomina;
            }
            catch (Exception ex) 
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.ConsultarNomina", $"ERROR al consultar la nómina correspondiente al mes de {Fecha}", ex.Message, ex.StackTrace);
                return null;
            }
        }
        private async Task<decimal?> PagarCuotaPrestamo (List<Prestamo> prestamos)
        {
            var session = logger.GetSessionData();
            try
            {
                // Si prestamos es null, retornar 0.00m directamente
                if (prestamos == null || prestamos.Count == 0)
                {
                    return 0.00m;
                }
                decimal? result = 0.00m;
                foreach(var prestamo in prestamos)
                {
                    var aPagar = nominaServices.PagarCuotaPrestamo(prestamo.CuotasPendientes, prestamo.TotalPendiente);
                    var nuevoPendiente = prestamo.TotalPendiente - aPagar;
                    var nuevaCuotaPendiente = prestamo.CuotasPendientes - 1;
                    prestamo.TotalPendiente = nuevoPendiente;
                    prestamo.CuotasPendientes = nuevaCuotaPendiente;
                    if (nuevaCuotaPendiente <= 0)
                    {
                        //si es menor o igual a 0, ya se pagó en su totalidad
                        prestamo.Pagado = 2;
                    }
                    context.Prestamos.Update(prestamo);
                    //generar historial de pagos
                    HistorialPago historialPago = new HistorialPago
                    {
                        IdEmpleado = prestamo.IdEmpleado,
                        IdPrestamo = prestamo.Id,
                        TotalPagado = aPagar,
                        FechaPago = DateTime.Now.Date,
                        TotalPendiente = nuevoPendiente
                    };
                    context.HistorialPagos.Add(historialPago);
                    await context.SaveChangesAsync();
                    await logger.LogTransaction(session.idEmpleado, session.company, "Nomina.PagarCuotaPrestamo", $"Se registró pago de cuota de prestamo al empleado {prestamo.IdEmpleado}", session.nombre);

                    result += aPagar.Value;
                }
                return result;
            }
            catch(Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.PagarCuotaPrestamo", $"ERROR al registrar pago de cuota de prestamo a empleado", ex.Message, ex.StackTrace);
                throw new InvalidOperationException($"Error al registrar pago de prestamo al empleado {ex.Message}", ex);
            }
        }
        private async Task<decimal?> ProyectarISR(int? id, decimal? salarioBruto, decimal? bonificacion)
        {
            var session = logger.GetSessionData();
            try
            {
                //traer el salario acumulado en el año del empleado
                var isrAcumulado = await context.Nominas.Where(e => e.IdEmpleado == id && e.Fecha.Year == DateTime.Now.Year)
                                                        .AsNoTracking()
                                                        .SumAsync(e => e.Isr);
                //llamar al servicio para hacer la proyectada mensual
                var proyectada = nominaServices.CalcularISR(salarioBruto,isrAcumulado,bonificacion);
                return proyectada;
            }
            catch (Exception ex)
            {
                await logger.LogError(session.idEmpleado, session.company, "Nomina.PagarCuotaPrestamo", $"ERROR al registrar proyectada de ISR a empleado : {id}", ex.Message, ex.StackTrace);
                throw new InvalidOperationException($"Error al realizar Proyectada de ISR al empleado: {id}: {ex.Message}", ex);
            }
        } 
    }
}
