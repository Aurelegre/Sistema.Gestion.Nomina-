function LimpiarFiltros(idForm) {
    var form = document.getElementById(idForm);
    document.getElementById("IdNombre").value = "";
    document.getElementById("IdPuesto").value = "";
    document.getElementById("IdDepartamento").value = "";
    form.submit();
}

