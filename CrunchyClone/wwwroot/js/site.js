$(document).ready(function () {
    // Adiciona transição suave ao hover dos cartões de filmes
    $('.movie-card').css('transition', 'transform 0.3s ease-in-out');

    // Aplica o efeito de escala no hover
    $('.movie-card').hover(
        function () {
            $(this).css('transform', 'scale(1.1)');
        },
        function () {
            $(this).css('transform', 'scale(1)');
        }
    );
});

