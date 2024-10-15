document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/SolicitudesAusencias/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Manejar valores nulos o vacíos
                        document.getElementById("ausenciaId").innerText = data.id || 'No disponible';
                        document.getElementById("ausenciaDetalle").innerText = data.detalle || 'No disponible';
                        document.getElementById("ausenciaEmpleado").innerText = data.empleado || 'No disponible';
                        document.getElementById("ausenciaSoli").innerText = data.fechaSoli || 'No disponible';
                        document.getElementById("ausenciaInicio").innerText = data.fechaInicio || 'No disponible';
                        document.getElementById("ausenciaFin").innerText = data.fechaFin || 'No disponible';
                        document.getElementById("ausenciaDias").innerText = data.dias !== null ? data.dias : 'No disponible';

                        const ESTADO_AUTORIZADO = 1;
                        const ESTADO_PENDIENTE = 2;
                        const ESTADO_DENEGADO = 3;

                        var estado = document.getElementById("ausenciaEstado");
                        var autorizado = document.getElementById("ausenciaJefe");
                        var fechaAuto = document.getElementById("ausenciaAutori");
                        var handler = document.getElementById("ausenciaHandle");
                        var Pautorizado = document.getElementById("PausenciaJefe");
                        var PfechaAuto = document.getElementById("PausenciaAutori");
                        var btnDenegate = document.getElementById("denegateID");
                        var btnAutorice = document.getElementById("autoriceID");

                        // Limpiar clases previas
                        estado.classList.remove('alert-success', 'alert-warning', 'alert-danger');

                        // Manejar estados
                        if (data.estado === ESTADO_AUTORIZADO) {
                            handler.innerText = "Autorización";
                            estado.innerText = "Autorizado";
                            estado.classList.add("alert-success");
                            autorizado.innerText = data.jefe && data.jefe.trim() !== "" ? data.jefe : 'No disponible';
                            Pautorizado.hidden = false; // Mostrar si está autorizado
                            fechaAuto.innerText = (data.fechaAut && data.fechaAut !== "0001-01-01") ? data.fechaAut : 'No disponible';
                            PfechaAuto.hidden = false;
                            btnAutorice.hidden = true;
                        } else if (data.estado === ESTADO_PENDIENTE) {
                            estado.innerText = "Pendiente";
                            estado.classList.add("alert-warning");
                            Pautorizado.hidden = true; // Ocultar si está pendiente
                            PfechaAuto.hidden = true;
                            handler.hidden = true;
                            btnAutorice.hidden = false;
                            btnDenegate.hidden = false;
                        } else if (data.estado === ESTADO_DENEGADO) {
                            handler.innerText = "Denegación";
                            estado.innerText = "Denegado";
                            estado.classList.add("alert-danger");
                            Pautorizado.hidden = true; // Ocultar si está denegado
                            PfechaAuto.hidden = true;
                            handler.hidden = false;
                            btnDenegate.hidden = true;
                        } else {
                            estado.innerText = "Estado desconocido";
                            estado.classList.add("alert-secondary");
                            Pautorizado.hidden = true; // Ocultar por defecto
                            PfechaAuto.hidden = true;
                            handler.hidden = true;
                            btnAutorice.hidden = true;
                            btnDenegate.hidden = true;
                        }

                        const TIPO_DEDUCIBLE = 1;
                        const TIPO_PENDIENTE = 2;
                        const TIPO_NODEDUCIBLE = 3;

                        var tipo = document.getElementById("ausenciaTipo");
                        var desc = document.getElementById("ausenciaDesc");
                        var Pdesc = document.getElementById("PausenciaDesc");

                        // Limpiar clases previas
                        tipo.classList.remove('alert-success', 'alert-warning', 'alert-danger');

                        if (data.tipo === TIPO_DEDUCIBLE) {
                            tipo.innerText = "Deducible";
                            tipo.classList.add("alert-success");
                            desc.innerText = data.deducible !== null ? data.deducible : 'No disponible';
                            Pdesc.hidden = false; // Mostrar si es deducible
                        } else if (data.tipo === TIPO_PENDIENTE) {
                            tipo.innerText = "Pendiente";
                            tipo.classList.add("alert-warning");
                            Pdesc.hidden = true; // Ocultar si está pendiente
                        } else if (data.tipo === TIPO_NODEDUCIBLE) {
                            tipo.innerText = "No Deducible";
                            tipo.classList.add("alert-success");
                            Pdesc.hidden = true; // Ocultar si no es deducible
                        } else {
                            tipo.innerText = "Estado desconocido";
                            tipo.classList.add("alert-secondary");
                            Pdesc.hidden = true; // Ocultar por defecto
                        }

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('solicitudAusenciaDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
                    .catch(error => {
                        console.error('Error al obtener los detalles del empleado:', error);
                    });

            }
            else if (action === "denegate") {
                DenegateSoli(id);
                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";
            }
            else if (action === "autorice") {
                AutoriceSoli(id);

                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";
            }
        });
    });
});

function DenegateSoli(id) {
    var idd;

    // Verificar si el parámetro id es null o undefined
    if (id === null || id === undefined) {
        idd = document.getElementById('ausenciaId').innerText; // Obtener el ID desde el span si no se pasa un parámetro
    } else {
        idd = id; // Usar el valor del parámetro id si existe
    }
    document.getElementById('IDsoli').value = idd
    // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('denegateConfirmationModal'));
    modal.show();
}
function AutoriceSoli(id) {
    var idd;

    // Verificar si el parámetro id es null o undefined
    if (id === null || id === undefined) {
        idd = document.getElementById('ausenciaId').innerText; // Obtener el ID desde el span si no se pasa un parámetro
    } else {
        idd = id; // Usar el valor del parámetro id si existe
    }
    document.getElementById('IDaut').value = idd
    // Mostrar el modal 
    var modal = new bootstrap.Modal(document.getElementById('autoriceConfirmationModal'));
    modal.show();
}