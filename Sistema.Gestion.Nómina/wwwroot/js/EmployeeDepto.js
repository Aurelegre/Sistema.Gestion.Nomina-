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




