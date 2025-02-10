mergeInto(LibraryManager.library,
{
    vibrate: function(duration)
    {
        if (typeof navigator.vibrate === "function") {
            navigator.vibrate(duration);
        }
    }
});