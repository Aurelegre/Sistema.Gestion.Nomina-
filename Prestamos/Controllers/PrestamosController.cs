using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prestamos.Entitys;
using Prestamos.Models;
using System.Text.Json;

namespace Prestamos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrestamosController : ControllerBase
    {
        private readonly SistemaGestionNominaContext _context;
        private readonly IMapper mapper;

        public PrestamosController(SistemaGestionNominaContext context, IMapper _mapper)
        {
            this._context = context;
            mapper = _mapper;
        }

        [HttpPost("register-load")]
        public async Task<IActionResult> RegisterLoan( PrestamoDTO request)
        {
            try 
            {
                var empleado = await _context.Empleados.SingleAsync(p=> p.Id == request.IdEmpleado);
                if (empleado == null)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "El Empleado no existe"
                    });
                }
                var prestamo = mapper.Map<Prestamo>(request);
                prestamo.TotalPendiente = request.Total;
                prestamo.CuotasPendientes = request.Cuotas;
                prestamo.Pagado = 0;
                _context.Add(prestamo);
                await _context.SaveChangesAsync();

                var tipoPrestamo = await _context.TiposPrestamos.SingleAsync(p=> p.Id == request.IdTipo);
                var logTrasaccion = new LogTransaccione
                {
                    IdEmpleado = request.IdEmpleado,
                    IdEmpresa = request.IdEmpresa,
                    Metodo = "RegisterLoan",
                    Descripcion = $"Se registró prestamo de tipo: {tipoPrestamo.Descripcion}, Por un total de: Q.{request.Total}, Empleado {empleado.Id}: {empleado.Nombre}",
                    Fecha = DateTime.Now
                };
                _context.Add(logTrasaccion);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    statusCode = 200,
                    message = "Se registró préstamo exitosamente"
                });
            }
            catch (Exception ex)
            {
                var logerror = new LogError
                {
                    Descripcion = $"Error al Registrar Prestamo del usuario",
                    Error = ex.ToString(),
                    StackTrace = ex.StackTrace,
                    Fecha = DateTime.Now,
                    Metodo = "RegisterLoan",
                    IdEmpleado = request.IdEmpleado,
                    IdEmpresa = request.IdEmpresa
                };
                _context.Add(logerror);
                await _context.SaveChangesAsync();
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Error al registrar préstamo"
                });
            }
            
        }

        [HttpPost("register-pay")]
        public async Task<IActionResult> RegisterPay(PayLoadDTO request) 
        {
            try
            {
                var empleado = await _context.Empleados.SingleAsync(p => p.Id == request.IdEmpleado);
                if(empleado == null)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "El Empleado no existe"
                    });
                }
                var prestamo = await _context.Prestamos.SingleAsync(p => p.Id == request.IdPrestamo);
                if (prestamo == null)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "El prestamo no existe"
                    });
                }

                var totalPendiente = prestamo.TotalPendiente - request.TotalPagado;
                prestamo.TotalPendiente = totalPendiente;
                prestamo.CuotasPendientes = prestamo.CuotasPendientes - 1;
                if(prestamo.TotalPendiente == 0 || prestamo.CuotasPendientes == 0)
                {
                    prestamo.Pagado = 1;
                }
                _context.Add(prestamo);
                await _context.SaveChangesAsync();
                var pago = new HistorialPago
                {
                    IdEmpleado = request.IdEmpleado,
                    IdPrestamo = request.IdPrestamo,
                    TotalPagado = request.TotalPagado,
                    FechaPago = DateTime.Now,
                    TotalPendiente = totalPendiente
                };
                _context.Add(pago);
                await _context.SaveChangesAsync();
                var tipoPrestamo = await _context.TiposPrestamos.SingleAsync(p => p.Id == prestamo.IdTipo);
                var logTrasaccion = new LogTransaccione
                {
                    IdEmpleado = request.IdEmpleado,
                    IdEmpresa = request.IdEmpresa,
                    Metodo = "RegisterPay",
                    Descripcion = $"Se registró pago de préstamo de tipo: {tipoPrestamo.Descripcion} por un total de: Q.{request.TotalPagado}, Empleado {empleado.Id}: {empleado.Nombre}",
                    Fecha = DateTime.Now
                };
                return Ok(new
                {
                    statusCode = 200,
                    message = "Se registró pago de préstamo exitosamente"
                });
            }
            catch (Exception ex)
            {
                var logerror = new LogError
                {
                    Descripcion = "Error al Registrar pago de Préstamo",
                    Error = ex.ToString(),
                    StackTrace = ex.StackTrace,
                    Fecha = DateTime.Now,
                    Metodo = "RegisterPay",
                    IdEmpleado = request.IdEmpleado,
                    IdEmpresa = request.IdEmpresa
                };
                _context.Add(logerror);
                await _context.SaveChangesAsync();
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Error al registrar Pago de préstamo"
                });
            }
           
        }

        [HttpGet("get-loads")]
        public async Task<IActionResult> GetLoads()
        {
            try
            {
                var prestamos = await _context.Prestamos.ToListAsync();

                return Ok(prestamos);
            }catch (Exception ex)
            {
                return Problem(detail: $"{ex.Message}\n {ex.StackTrace} ", statusCode: 500);
            }
            
        }
    }
}
