// Check if mobile
window.checkIfMobile = () => {
    return window.innerWidth <= 992;
};

// Theme Manager
window.themeManager = {
    setTheme: function (isDark) {
        localStorage.setItem('darkTheme', isDark);
        if (isDark) {
            document.documentElement.classList.add('dark-theme');
            document.documentElement.classList.remove('light-theme');
            document.body.classList.add('dark-theme');
            document.body.classList.remove('light-theme');
        } else {
            document.documentElement.classList.add('light-theme');
            document.documentElement.classList.remove('dark-theme');
            document.body.classList.add('light-theme');
            document.body.classList.remove('dark-theme');
        }
    },
    getTheme: function () {
        const theme = localStorage.getItem('darkTheme');
        return theme === 'true';
    }
};

// Initialize MainLayout
window.initializeMainLayout = (dotNetHelper) => {
    if (!dotNetHelper) {
        console.warn('dotNetHelper is null');
        return;
    }

    let resizeTimeout;

    const handleResize = () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            const isMobile = window.innerWidth <= 992;
            try {
                dotNetHelper.invokeMethodAsync('UpdateMobileState', isMobile)
                    .catch(error => console.error('Error invoking UpdateMobileState:', error));
            } catch (error) {
                console.error('Exception calling invokeMethodAsync:', error);
            }
        }, 250);
    };

    window.addEventListener('resize', handleResize);
    handleResize();

    window._mainLayoutCleanup = () => {
        window.removeEventListener('resize', handleResize);
    };
};

// Dispose MainLayout
window.disposeMainLayout = () => {
    if (window._mainLayoutCleanup) {
        try {
            window._mainLayoutCleanup();
        } catch (error) {
            console.error('Error during cleanup:', error);
        }
        delete window._mainLayoutCleanup;
    }
};

// File download helper
window.downloadFileFromBytes = function (fileName, byteArray) {
    const blob = new Blob([new Uint8Array(byteArray)], {
        type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

// Initialize theme on load
document.addEventListener('DOMContentLoaded', () => {
    const isDark = themeManager.getTheme();
    themeManager.setTheme(isDark);
});

console.log('Site.js loaded successfully');