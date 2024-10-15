$(document).ready(function () {
    // Você pode adicionar animações ou efeitos aqui, se necessário
    $('.movie-card').hover(
        function () {
            $(this).css('transform', 'scale(1.1)');
        },
        function () {
            $(this).css('transform', 'scale(1)');
        }
    );
});
