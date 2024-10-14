document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Ausencias/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("ausenciaId").innerText = data.id || data.Id;
                        document.getElementById("ausenciaDetalle").innerText = data.detalle || 'No disponible';
                        document.getElementById("ausenciaSoli").innerText = data.fechaSoli || 'No disponible';
                        document.getElementById("ausenciaInicio").innerText = data.fechaInicio || 'No disponible';
                        document.getElementById("ausenciaFin").innerText = data.fechaFin || 'No disponible';
                        document.getElementById("ausenciaDias").innerText = data.dias || 'No disponible';

                        //verificar el estado con constantes para mejor legibilidad
                        const ESTADO_AUTORIZADO = 1;
                        const ESTADO_PENDIENTE = 2;
                        const ESTADO_DENEGADO = 3;

                        var estado = document.getElementById("ausenciaEstado");
                        var autorizado = document.getElementById("ausenciaJefe");
                        var fechaAuto = document.getElementById("ausenciaAutori");
                        var handler = document.getElementById("ausenciaHandle");
                        var Pautorizado = document.getElementById("PausenciaJefe");
                        var PfechaAuto = document.getElementById("PausenciaAutori");

                        // Limpiamos las clases previas
                        estado.classList.remove('alert-success', 'alert-warning', 'alert-danger');

                        if (data.estado === ESTADO_AUTORIZADO) {
                            handler.innerText = "Autorización"
                            estado.innerText = "Autorizado";
                            estado.classList.add("alert-success");
                            autorizado.innerText = data.jefe || 'No disponible';
                            Pautorizado.hidden = false; // Mostrar si está autorizado
                            fechaAuto.innerText = data.fechaAut || 'No disponible'
                            PfechaAuto.hidden = false;
                        }
                        else if (data.estado === ESTADO_PENDIENTE) {
                            estado.innerText = "Pendiente";
                            estado.classList.add("alert-warning");
                            Pautorizado.hidden = true; // Ocultar si está pendiente
                            PfechaAuto.hidden = true;
                            handler.hidden = true
                        }
                        else if (data.estado === ESTADO_DENEGADO) {
                            handler.innerText = "Denegación";
                            estado.innerText = "Denegado";
                            estado.classList.add("alert-danger");
                            Pautorizado.hidden = true; // Ocultar si está denegado
                            PfechaAuto.hidden = true;
                            handler.hidden = false;
                        }
                        else {
                            estado.innerText = "Estado desconocido";
                            estado.classList.add("alert-secondary");
                            Pautorizado.hidden = true; // Ocultar por defecto
                            PfechaAuto.hidden = true;
                            handler.hidden = true;
                        }

                        
                        const TIPO_DEDUCIBLE = 1;
                        const TIPO_PENDIENTE = 2;
                        const TIPO_NODEDUCIBLE = 3;

                        var tipo = document.getElementById("ausenciaTipo");
                        var desc = document.getElementById("ausenciaDesc");
                        var Pdesc = document.getElementById("PausenciaDesc");

                        // Limpiamos las clases previas
                        tipo.classList.remove('alert-success', 'alert-warning', 'alert-danger');

                        if (data.tipo === TIPO_DEDUCIBLE) {
                            tipo.innerText = "Deducible";
                            tipo.classList.add("alert-success");
                            desc.innerText = data.deducible || 'No disponible';
                            Pdesc.hidden = false; // Mostrar si es deducible
                        }
                        else if (data.tipo === TIPO_PENDIENTE) {
                            tipo.innerText = "Pendiente";
                            tipo.classList.add("alert-warning");
                            Pdesc.hidden = true; // Ocultar si está pendiente
                        }
                        else if (data.tipo === TIPO_NODEDUCIBLE) {
                            tipo.innerText = "No Deducible";
                            tipo.classList.add("alert-success");
                            Pdesc.hidden = true; // Ocultar si no es deducible
                        }
                        else {
                            tipo.innerText = "Estado desconocido";
                            tipo.classList.add("alert-secondary");
                            Pdesc.hidden = true; // Ocultar por defecto
                        }
                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('ausenciaDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
                    .catch(error => {
                        console.error('Error al obtener los detalles del empleado:', error);
                    });

            }
            else if (action === "editar") {
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
            }
            else if (action === "eliminar") {
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
function createAusencia() {

    // Mostrar el modal
    var modal = new bootstrap.Modal(document.getElementById('createAusenciaModal'));
    modal.show();
}