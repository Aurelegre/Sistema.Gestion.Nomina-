document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Employees/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("employeeId").innerText = data.id || data.Id;
                        document.getElementById("employeeName").innerText = data.nombre || data.Nombre;
                        document.getElementById("employeeDpi").innerText = data.dpi || data.DPI;
                        document.getElementById("employeePuesto").innerText = data.puesto || data.Puesto;
                        document.getElementById("employeeDepartamento").innerText = data.departamento || data.Departamento;
                        document.getElementById("employeeSueldo").innerText = data.sueldo || data.Sueldo;
                        document.getElementById("employeeFechaContratado").innerText = data.fechaContratado || data.FechaContratado;
                        document.getElementById("employeeUsuario").innerText = data.usuario || 'Sin asignar';

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('employeeDetailModal'));
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
                        document.getElementById("editSueldo").value = data.sueldo || data.Sueldo;

                        // Llenar combobox de puestos
                        var editPuesto = document.getElementById("editPuesto");
                        editPuesto.innerHTML = "";  // Limpiar opciones actuales
                        data.puestos.forEach(function (puesto) {
                            editPuesto.append(new Option(puesto.descripcion, puesto.id, data.idPuesto === puesto.id));
                        });

                        // Llenar combobox de departamentos
                        var editDepartamento = document.getElementById("editDepartamento");
                        editDepartamento.innerHTML = "";  // Limpiar opciones actuales
                        data.departamento.forEach(function (departamento) {
                            editDepartamento.append(new Option(departamento.descripcion, departamento.id, data.IdDepto === departamento.id));
                        });

                        // Llenar combobox de Usuario
                        var editUsuario = document.getElementById("editUsuario");
                            editUsuario.innerHTML = "";  // Limpiar opciones actuales

                        if (data.usuario) {
                            editUsuario.append(new Option(data.usuario, 0));
                        } else {
                            editUsuario.append(new Option("Usuario sin Asingar", 0));
                        }
                            
                        
                        if (data.usuarios && Array.isArray(data.usuarios) && data.usuarios.length > 0) {
                            data.usuarios.forEach(function (usuario) {
                                editUsuario.append(new Option(usuario.usuario1, usuario.id));
                            });
                        } else {
                            var noUsuariosOption = new Option("No hay usuarios disponibles", "", true, false);
                            noUsuariosOption.disabled = true;
                            editUsuario.append(noUsuariosOption);
                        }
                        
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
            }
        });
    });

    var errorMessage = document.getElementById('message');
    if (errorMessage) {
        setTimeout(function () {
            errorMessage.style.display = 'none';
        }, 5000); // Oculta el mensaje después de 5 segundos
    }
});

function fetchEmployeeData() {
    fetch('/Employees/Create/') // Reemplaza con la URL correcta de tu controlador
        .then(response => response.json())
        .then(data => {
            // Llenar el combobox de Usuarios
            var usuariosSelect = document.getElementById('IdUsuario');
            usuariosSelect.innerHTML = '<option value="">Seleccione un Usuario</option>';
            if (data.usuarios && Array.isArray(data.usuarios) && data.usuarios.length > 0) {
                data.usuarios.forEach(function (usuario) {
                    usuariosSelect.append(new Option(usuario.usuario1, usuario.Id));
                });
            } else {
                var noUsuariosOption = new Option("No hay usuarios disponibles", "", true, false);
                noUsuariosOption.disabled = true;
                usuariosSelect.append(noUsuariosOption);
            }
            

            // Llenar el combobox de Departamentos
            var departamentosSelect = document.getElementById('IdDepartamento');
            departamentosSelect.innerHTML = '<option value="">Seleccione un Departamento</option>';
            data.departamentos.forEach(function (departamento) {
                departamentosSelect.append(new Option(departamento.descripcion, departamento.id));
            });
            // Mostrar el modal
            var modal = new bootstrap.Modal(document.getElementById('createEmployeeModal'));
            modal.show();
        });
}
function onDepartamentoChange(selectElement,idpuesto) {
    var departamentoId = selectElement.value; // Obtener el ID del departamento seleccionado
    if (departamentoId) {
        fetchPuestosData(departamentoId, idpuesto); // Llamar a la función para llenar el combobox de puestos
    } else {
        // Si no se selecciona ningún departamento, vaciar el combobox de puestos
        var puestosSelect = document.getElementById(idpuesto);
        puestosSelect.innerHTML = '<option value="">Debe seleccionar un Puesto</option>';
    }
}
function fetchPuestosData(id,idpuesto) {
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
            else
            {
                var noUsuariosOption = new Option("No hay puestos asignados", "", true, false);
                noUsuariosOption.disabled = true;
                puestosSelect.append(noUsuariosOption);
            }
           
        });
}
