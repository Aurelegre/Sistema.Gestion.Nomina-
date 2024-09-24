document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('passwordRecoveryForm');
    const errorDiv = document.getElementById('error');
    const newPassword = document.getElementById('newPassword');
    const confirmPassword = document.getElementById('confirmPassword');

    form.addEventListener('submit', function (event) {
        event.preventDefault();

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
        
            
        
        // Enviar el formulario si todo está correcto
        form.submit();
    });
});
