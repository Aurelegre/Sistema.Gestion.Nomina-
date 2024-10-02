document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');
            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Departamento/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("deptoId").innerText = data.id || data.Id;
                        document.getElementById("deptoName").innerText = data.Descripcion || data.descripcion;
                        document.getElementById("deptoJefe").innerText = data.jefe || 'Sin asignar';

                        // Limpiar los campos anteriores
                        var contenedor = document.getElementById('puestosDepto');
                        contenedor.innerHTML = '';

                        // Mostrar los puestos
                        if (data.puestos && data.puestos.length > 0) {
                            var count = 1;
                            data.puestos.forEach(function (puesto) {
                                contenedor.innerHTML += `
                                    <p>
                                        <strong> &nbsp; ${count}.</strong><span"> ${puesto.descripcion}</span>
                                    </p>
                                `;
                                count = count + 1;
                            });
                        } else {
                            contenedor.innerHTML = '<p>Sin Puestos Asignados.</p>';
                        }

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('deptoDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
                    .catch(error => {
                        console.error('Error al obtener los detalles del empleado:', error);
                    });

            }
            if (action === "editar") {
                fetch('/Departamento/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Llenar los campos de edición del empleado en el modal
                        document.getElementById("editId").value = data.id || data.Id;
                        document.getElementById("editDescripcion").value = data.descripcion || data.Descripcion;

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('editDeptoModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    });
            }
            else if (action === "eliminar") {
                document.getElementById("deleteId").value = id;
                // Mostrar el modal
                var modal = new bootstrap.Modal(document.getElementById('deleteConfirmationModal'));
                modal.show();


                // Restablecer el valor del combobox a la opción predeterminada
                dropdown.value = "Seleccionar";
            }
            else if (action === "permisos") {
                window.location.href = '/Permission/Index?idRol=' + id;
                dropdown.value = "Seleccionar";
            }
        });
    });
});
function fetchDepto() {
    // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('createDeptoModal'));
    modal.show();
}