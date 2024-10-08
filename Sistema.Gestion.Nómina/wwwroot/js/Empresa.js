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

            }
            else if (action === "editar") {
                fetch('/Empresa/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Llenar los campos de edición del empleado en el modal
                        document.getElementById("editId").value = data.id || data.Id;
                        document.getElementById("editIdUser").value = data.idUsuario;
                        document.getElementById("editNombre").value = data.nombre;
                        document.getElementById("editDireccion").value = data.direccion;
                        document.getElementById("editTelefono").value = data.telefono;
                        document.getElementById("editUsuario").value = data.usuario;

                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('editEmpresaModal'));
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

function ConfirmPassword(idform, password, confirmpass,error) {
    const errorDiv = document.getElementById(error);
    const newPassword = document.getElementById(password);
    const confirmPassword = document.getElementById(confirmpass);

    
        // Limpiar mensaje de error
        errorDiv.style.display = 'none';
        errorDiv.innerText = '';

        // Validación de contraseña
        if (newPassword.value !== confirmPassword.value) {
            errorDiv.style.display = 'block';
            errorDiv.innerText = 'Las contraseñas no coinciden. Por favor, inténtalo de nuevo.';
            setTimeout(function () {
                errorDiv.style.display = 'none';
            }, 5000); // Oculta el mensaje después de 5 segundos
            return;
        }
}

function confirmEdit(idForm,idModal, idForm2) {
    const form = document.getElementById(idForm);
    const form2 = document.getElementById(idForm2);
    var modal = new bootstrap.Modal(document.getElementById(idModal));
    modal.show();
    form2.addEventListener('submit', function (event) {
        event.preventDefault();
        form.submit();
    });
}
function createEmpresa() {
           // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('createEmpresaModal'));
    modal.show();
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


