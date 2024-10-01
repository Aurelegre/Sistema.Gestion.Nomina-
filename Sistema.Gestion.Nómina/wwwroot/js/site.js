document.addEventListener('DOMContentLoaded', function () {
    const toggleButton = document.getElementById('menu-toggle');
    const wrapper = document.getElementById('wrapper');

    // Get the state from local storage and apply it
    if (localStorage.getItem('sidebarToggled') === 'true') {
        wrapper.classList.add('toggled');
    }

    toggleButton.addEventListener('click', function () {
        wrapper.classList.toggle('toggled');

        // Save the state to local storage
        const isToggled = wrapper.classList.contains('toggled');
        localStorage.setItem('sidebarToggled', isToggled);
    });
});
