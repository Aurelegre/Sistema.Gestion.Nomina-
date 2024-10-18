document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "historial") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Prestamo/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("idCuotas").innerText = data.cPendientes;
                        document.getElementById("idTotalP").innerText = 'Q. ' + data.totalPediente || 'No disponible';

                        //llenar el body de tabla con id bodytable
                        var body = document.getElementById("bodytable");
                        var conter = 1;
                        body.innerHTML = '';
                        data.pagos.forEach(function (pago) {

                            body.innerHTML += `  <td>${conter}</td>
                                                <td>${pago.fecha}</td>
                                                <td>${pago.totalPagado}</td>
                                                <td>${pago.totalPediente}</td>`
                                ;
                            conter++;
                        });

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('prestamoDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
                    .catch(error => {
                        console.error('Error al obtener los detalles del empleado:', error);
                    });

            } else if (action === "eliminar") {
                document.getElementById("deleteId").value = id;
                // Mostrar el modal
                var modal = new bootstrap.Modal(document.getElementById('deleteConfirmationModal'));
                modal.show();

                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";
            }
            
        });
    });
});
function createPrestamo() {

    // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('createPrestamoModal'));
    modal.show();
}
function confirmCreate(idForm, idForm2) {
    const form = document.getElementById(idForm);
    const form2 = document.getElementById(idForm2);
    var modal = new bootstrap.Modal(document.getElementById('createConfirmationModal'));
    modal.show();
    form2.addEventListener('submit', function (event) {
        event.preventDefault();
        form.submit();
    });
}