document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.actions-dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('change', function () {
            var action = this.value;
            var id = this.options[this.selectedIndex].getAttribute('data-id');

            if (action === "detalle") {
                // Hacer la petición para obtener los detalles del empleado
                fetch('/Nomina/Details/' + id)
                    .then(response => response.json())
                    .then(data => {
                        // Mostrar los detalles del empleado en el modal
                        document.getElementById("idnomina").value = data.id;
                        document.getElementById("employeeName").innerText = data.nombreEmpleado;
                        document.getElementById("employeeDepartamento").innerText = data.departamento;
                        document.getElementById("employeePuesto").innerText = data.puesto;
                        document.getElementById("employeeSueldo").innerText = data.sueldo;
                        document.getElementById("employeeExtra").innerText = data.sueldoExtra;
                        document.getElementById("employeeComis").innerText = data.comisiones;
                        document.getElementById("employeeBonis").innerText = data.bonificaciones;
                        document.getElementById("employeeAgui").innerText = data.aguinaldoBono;
                        document.getElementById("employeeIngre").innerText = data.otrosIngresos;
                        document.getElementById("employeeDeven").innerText = data.totalDevengado;
                        document.getElementById("employeeIGSS").innerText = data.igss;
                        document.getElementById("employeeISR").innerText = data.isr;
                        document.getElementById("employeePres").innerText = data.prestamos;
                        document.getElementById("employeeCred").innerText = data.creditos;
                        document.getElementById("employeeAnti").innerText = data.anticipos;
                        document.getElementById("employeeDesc").innerText = data.otrosDesc;
                        document.getElementById("employeeTDesc").innerText = data.totalDescuentos;
                        document.getElementById("employeeLiquido").innerText = data.totalLiquido;


                        // Mostrar el modal
                        var modal = new bootstrap.Modal(document.getElementById('NominaemployeeDetailModal'));
                        modal.show();

                        // Restablecer el valor del combobox a la opción predeterminada
                        dropdown.value = "Seleccionar";
                    })
            }
        });
    });
});

function LimpiarFiltros(idForm) {
    var form = document.getElementById(idForm);
    document.getElementById("IdNombre").value = "";
    document.getElementById("IdPuesto").value = "";
    document.getElementById("IdDepartamento").value = "";
    form.submit();
}



