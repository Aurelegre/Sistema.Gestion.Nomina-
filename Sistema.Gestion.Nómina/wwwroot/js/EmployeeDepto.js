document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/EmployeeDepto/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("employeeId").innerText = data.id || data.Id;
                        document.getElementById("employeeName").innerText = data.nombre || data.Nombre;
                        document.getElementById("employeeApellido").innerText = data.apellidos || data.Apellidos;
                        document.getElementById("employeeDpi").innerText = data.dpi || data.DPI;
                        document.getElementById("employeePuesto").innerText = data.puesto || data.Puesto;
                        document.getElementById("employeeDepartamento").innerText = data.departamento || data.Departamento;
                        document.getElementById("employeeSueldo").innerText = data.sueldo || data.Sueldo;
                        document.getElementById("employeeFechaContratado").innerText = data.fechaContratado || data.FechaContratado;


                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('employeeDeptoDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    });
            }
            else if (action === "extras") {
                
                document.getElementById("employeeHorasId").value = id;

                        // Mostrar el modal
                var modal = new bootstrap.Modal(document.getElementById('createHorasModal'));
                modal.show();
                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";

            }
            else if (action === "comisiones") {

                document.getElementById("employeeComiId").value = id;

                // Mostrar el modal
                var modal = new bootstrap.Modal(document.getElementById('createComisionModal'));
                modal.show();
                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";

            }
            else if (action === "eliminar") {
                document.getElementById("deleteId").value = id;
                // Mostrar el modal
                var modal = new bootstrap.Modal(document.getElementById('deleteConfirmationModal'));
                modal.show();


                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";
            } else if (action === "desbloquear") {
                document.getElementById("unlockId").value = id;
                // Mostrar el modal
                var modal = new bootstrap.Modal(document.getElementById('unlockConfirmationModal'));
                modal.show();


                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";
            }
        });
    });


});
function validarTiempo(idform) {
    // Obtener el valor del campo de tiempo
    var tiempoInput = document.getElementById('IdHoras').value;
    var message = document.getElementById('idmessages');
    const form = document.getElementById(idform);
    // Verificar que el input tenga un valor
    if (!tiempoInput) {
        message.innerHTML = 'No se pueden registrar horas menores o iguales a 30 minutos.'
        message.hidden = false;
        setTimeout(function () {
            message.hidden = true;
        }, 5000);
        return false;  // Prevenir el envío del formulario
    }

    // Separar horas y minutos
    var partes = tiempoInput.split(':');
    var horas = parseInt(partes[0], 10);
    var minutos = parseInt(partes[1], 10);

    // Convertir todo a minutos
    var totalMinutos = (horas * 60) + minutos;

    // Verificar si es mayor a 30 minutos
    if (totalMinutos <= 30) {
        
        message.innerHTML = 'No se pueden registrar horas menores o iguales a 30 minutos.'
        message.hidden = false;
        setTimeout(function () {
            message.hidden = true;
        }, 5000);
        return false
    }
    if (form.checkValidity()) {
        // Si es válido
        form.submit();
    } else {
        // Si no es válido, muestra los errores nativos de HTML5
        form.reportValidity();
    }
    
    return true;  // Permitir el envío si cumple la validación
}




