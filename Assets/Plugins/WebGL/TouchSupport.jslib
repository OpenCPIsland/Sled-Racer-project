mergeInto(LibraryManager.library, {
    GetTouchCount: function() {
        if (typeof window.touches !== 'undefined') return window.touches.length;
        return 0;
    },
    GetTouchX: function(index) {
        if (typeof window.touches !== 'undefined' && window.touches.length > index) {
            return window.touches[index].clientX;
        }
        return 0;
    },
    GetTouchY: function(index) {
        if (typeof window.touches !== 'undefined' && window.touches.length > index) {
            return window.touches[index].clientY;
        }
        return 0;
    }
});

if (typeof window !== 'undefined') {
    window.touches = [];
    window.addEventListener('touchstart', function(e){ window.touches = e.touches; }, true);
    window.addEventListener('touchmove', function(e){ window.touches = e.touches; }, true);
    window.addEventListener('touchend', function(e){ window.touches = e.touches; }, true);
    window.addEventListener('touchcancel', function(e){ window.touches = e.touches; }, true);
}