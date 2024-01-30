mergeInto(LibraryManager.library, {
    Hello: function () {
        window.alert("Hello, world!");
    },

    StringReturn: function () {
        var returnStr = "Wavenet";
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
    }
});