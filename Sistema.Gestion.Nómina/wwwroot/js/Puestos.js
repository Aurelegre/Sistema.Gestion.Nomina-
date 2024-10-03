document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');
            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Puesto/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("puestoId").innerText = data.id || data.Id;
                        document.getElementById("puestoName").innerText = data.Descripcion || data.descripcion;
                        document.getElementById("puestoDepto").innerText = data.departamento || 'Sin asignar';

                        // Limpiar los campos anteriores
                        var contenedor = document.getElementById('empleadoPuesto');
                        contenedor.innerHTML = '';

                        // Mostrar los Empleados
                        if (data.empleados && data.empleados.length > 0) {
                            var count = 1;
                            data.empleados.forEach(function (empleado) {
                                contenedor.innerHTML += `
                                    <p>
                                        <strong> &nbsp; ${count}.</strong><span"> ${empleado}</span>
                                    </p>
                                `;
                                count = count + 1;
                            });
                        } else {
                            contenedor.innerHTML = '<p>Sin Empleados Asignados.</p>';
                        }

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('puestoDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
                    .catch(error => {
                        console.error('Error al obtener los detalles del empleado:', error);
                    });

            }
            if (action === "editar") {
                fetch('/Puesto/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Llenar los campos de edición del empleado en el modal
                        document.getElementById("editId").value = data.id || data.Id;
                        document.getElementById("editDescripcion").value = data.descripcion || data.Descripcion;

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('editPuestoModal'));
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
function fetchCreatePuesto() {
    fetch('/Puesto/GetDeptos/') 
        .then(response => response.json())
        .then(data => {


            // Llenar el combobox de Departamentos
            var departamentosSelect = document.getElementById('IdDepartamento');
            departamentosSelect.innerHTML = '<option value="">Seleccione un Departamento</option>';
            if (data.departamentos && Array.isArray(data.departamentos) && data.departamentos.length > 0) {
                data.departamentos.forEach(function (departamento) {
                    departamentosSelect.append(new Option(departamento.descripcion, departamento.id));
                });
            } else {
                var noDepartamentOption = new Option("No hay departamentos disponibles", "", true, false);
                noDepartamentOption.disabled = true;
                departamentosSelect.append(noDepartamentOption);
            }
        });
    // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('createPuestoModal'));
    modal.show();
}