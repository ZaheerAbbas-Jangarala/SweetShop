const signUpButton = document.getElementById('signUp');
const signInButton = document.getElementById('signIn');
const container = document.getElementById('container');
const msg = document.getElementById('msg');

// Toggle panels
signUpButton.addEventListener('click', () => container.classList.add("right-panel-active"));
signInButton.addEventListener('click', () => container.classList.remove("right-panel-active"));

// -------------- REGISTER --------------
$('#registerForm').submit(function (e) {
    e.preventDefault();

    const name = $('#registerForm input[name="Name"]').val().trim();
    const email = $('#registerForm input[name="Email"]').val().trim();
    const password = $('#registerForm input[name="Password"]').val().trim();

    // Client-side validation
    if (!name) {
        showMessage('Name is required!', 'danger');
        return;
    }
    if (!email) {
        showMessage('Email is required!', 'danger');
        return;
    }
    if (!password) {
        showMessage('Password is required!', 'danger');
        return;
    }

    $.ajax({
        url: '/Auth/Register',
        type: 'POST',
        data: $(this).serialize(),
        success: function (res) {
            showMessage(res.message || 'Registration successful!', 'success');
            $('#registerForm')[0].reset();
            container.classList.add("right-panel-active"); // Stay on Sign Up panel
        },
        error: function (xhr) {
            showMessage(xhr.responseJSON?.error || 'Registration failed.', 'danger');
            container.classList.add("right-panel-active");
        }
    });
});

// -------------- LOGIN --------------
$('#loginForm').submit(function (e) {
    e.preventDefault();

    const email = $('#loginForm input[name="Email"]').val().trim();
    const password = $('#loginForm input[name="Password"]').val().trim();

    $.ajax({
        url: '/Auth/Login',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ Email: email, Password: password }),
        success: function (res) {
            showMessage(res.message || 'Login successful!', 'success');

            // Role-based redirect (case-insensitive check)
            if (res.user && res.user.role && res.user.role.toLowerCase() === 'admin') {
                setTimeout(() => window.location.href = '/Sweets/AdminIndex', 1000);
            } else {
                setTimeout(() => window.location.href = '/Sweets/Shop', 1000);
            }
        },
        error: function (xhr) {
            showMessage(xhr.responseJSON?.error || 'Invalid credentials.', 'danger');
        }
    });
});

// -------------- HELPER FUNCTION --------------
function showMessage(text, type) {
    msg.style.display = 'block';
    msg.className = `alert alert-${type}`;
    msg.innerText = text;

    setTimeout(() => $('#msg').fadeOut(), 3000);
}
