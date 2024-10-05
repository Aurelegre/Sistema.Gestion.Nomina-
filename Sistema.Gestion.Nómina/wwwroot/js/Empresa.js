document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Empresa/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("empresaId").innerText = data.id || data.Id;
                        document.getElementById("empresaName").innerText = data.nombre || data.Nombre;
                        document.getElementById("empresaDireccion").innerText = data.direccion;
                        document.getElementById("empresaTel").innerText = data.telefono;
                        document.getElementById("empresaAdmin").innerText = data.administrador;
                        document.getElementById("empresaUsuario").innerText = data.usuario;


                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('empresaDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
                    .catch(error => {
                        console.error('Error al obtener los detalles del empleado:', error);
                    });

            } else if (action === "editar") {
                fetch('/Employees/Update/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Llenar los campos de edición del empleado en el modal
                        document.getElementById("editId").value = data.id || data.Id;
                        document.getElementById("editNombre").value = data.nombre || data.Nombre;
                        document.getElementById("editApellidos").value = data.apellidos || data.Apellidos;
                        document.getElementById("editSueldo").value = data.sueldo || data.Sueldo;
                        document.getElementById("editUsuario").value = data.usuario || data.Usuario;

                        // Llenar combobox de puestos
                        var editPuesto = document.getElementById("editPuesto");
                        editPuesto.innerHTML = "";  // Limpiar opciones actuales
                        data.puestos.forEach(function (puesto) {
                            var isSelected = data.idPuesto === puesto.id;
                            editPuesto.append(new Option(puesto.descripcion, puesto.id, isSelected, isSelected));
                        });

                        // Llenar combobox de departamentos
                        var editDepartamento = document.getElementById("editDepartamento");
                        editDepartamento.innerHTML = "";  // Limpiar opciones actuales
                        data.departamento.forEach(function (departamento) {
                            var isSelected = data.idDepto === departamento.id;
                            editDepartamento.append(new Option(departamento.descripcion, departamento.id, isSelected, isSelected));
                        });

                        // Llenar combobox de departamentos
                        var editRol = document.getElementById("editRol");
                        editRol.innerHTML = "";  // Limpiar opciones actuales
                        data.roles.forEach(function (rol) {
                            editRol.append(new Option(rol.descripcion, rol.id, data.idRol === rol.id, data.idRol === rol.id));
                        });

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('editEmployeeModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    });
            } else if (action === "eliminar") {
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

function fetchEmployeeData() {
    fetch('/Employees/Create/') // Reemplaza con la URL correcta de tu controlador
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

            // Llenar el combobox de Roles
            var rolesSelect = document.getElementById('IdRol');
            rolesSelect.innerHTML = '<option value="">Seleccione un Rol</option>';
            if (data.roles && Array.isArray(data.roles) && data.roles.length > 0) {
                data.roles.forEach(function (rol) {
                    rolesSelect.append(new Option(rol.descripcion, rol.id));
                });
            } else {
                var noRolOption = new Option("No hay roles disponibles", "", true, false);
                noRolOption.disabled = true;
                rolesSelect.append(noRolOption);
            }

            // Mostrar el modal
            var modal = new bootstrap.Modal(document.getElementById('createEmployeeModal'));
            modal.show();
        });
}
function onDepartamentoChange(selectElement, idpuesto) {
    var departamentoId = selectElement.value; // Obtener el ID del departamento seleccionado
    if (departamentoId) {
        fetchPuestosData(departamentoId, idpuesto); // Llamar a la función para llenar el combobox de puestos
    } else {
        // Si no se selecciona ningún departamento, vaciar el combobox de puestos
        var puestosSelect = document.getElementById(idpuesto);
        puestosSelect.innerHTML = '<option value="">Debe seleccionar un Puesto</option>';
    }
}
function fetchPuestosData(id, idpuesto) {
    fetch('/Employees/GetPuestos/' + id) // Reemplaza con la URL correcta de tu controlador
        .then(response => response.json())
        .then(data => {
            // Llenar el combobox de Puestos
            var puestosSelect = document.getElementById(idpuesto);
            puestosSelect.innerHTML = '<option value="">Seleccione un Puesto</option>';

            if (data.puestos && Array.isArray(data.puestos) && data.puestos.length > 0) {
                data.puestos.forEach(function (puesto) {
                    puestosSelect.append(new Option(puesto.descripcion, puesto.id));
                });
            }
            else {
                var noUsuariosOption = new Option("No hay puestos asignados", "", true, false);
                noUsuariosOption.disabled = true;
                puestosSelect.append(noUsuariosOption);
            }

        });
}


