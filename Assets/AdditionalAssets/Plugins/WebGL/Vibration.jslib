mergeInto(LibraryManager.library,
{
    vibrate: function(duration)
    {
        if (typeof navigator.vibrate === "function") {
            navigator.vibrate(duration);
        }
    },
    IsReleaseVersion: function ()
	{
		return IsRelease();
	},
    GetBuildNumber: function()
    {
       	return getBuildNumber();
    }
});