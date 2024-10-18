document.addEventListener('DOMContentLoaded', function () {
    const toggleButton = document.getElementById('menu-toggle');
    const wrapper = document.getElementById('wrapper');

    // Get the state from local storage and apply it
    //if (localStorage.getItem('sidebarToggled') === 'true') {
    //    wrapper.classList.add('toggled');
    //}

    toggleButton.addEventListener('click', function () {
        wrapper.classList.toggle('toggled');

        // Save the state to local storage
        const isToggled = wrapper.classList.contains('toggled');
        localStorage.setItem('sidebarToggled', isToggled);
    });
});
function closeOtherAccordions(openAccordionId) {
    // Selecciona todos los elementos con la clase 'accordion-collapse'
    var accordions = document.querySelectorAll('.accordion-collapse');

    // Itera sobre todos los acordeones
    accordions.forEach(function (accordion) {
        // Si el acordeón actual no es el que tiene el ID proporcionado
        if (accordion.id !== openAccordionId) {
            var bsCollapse = new bootstrap.Collapse(accordion, {
                toggle: false // No queremos que cambie de estado automáticamente
            });

            // Verifica si el acordeón está actualmente desplegado y lo colapsa
            if (accordion.classList.contains('show')) {
                bsCollapse.hide();
            }
        }
    });
}

