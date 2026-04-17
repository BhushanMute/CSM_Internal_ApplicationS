// wwwroot/js/mainLayout.js

// ==========================================
// THEME MANAGER
// ==========================================
window.themeManager = {
    storageKey: 'ems-theme-preference',

    getTheme: function () {
        const saved = localStorage.getItem(this.storageKey);

        if (saved !== null) {
            return saved === 'dark';
        }

        // Check system preference
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return true;
        }

        return false;
    },

    setTheme: function (isDark) {
        localStorage.setItem(this.storageKey, isDark ? 'dark' : 'light');
        this.applyTheme(isDark);
    },

    applyTheme: function (isDark) {
        const root = document.documentElement;
        const body = document.body;

        if (isDark) {
            root.classList.add('dark-theme');
            root.classList.remove('light-theme');
            body.classList.add('dark-theme');
            body.classList.remove('light-theme');
        } else {
            root.classList.add('light-theme');
            root.classList.remove('dark-theme');
            body.classList.add('light-theme');
            body.classList.remove('dark-theme');
        }

        // Update meta theme-color
        const metaThemeColor = document.querySelector('meta[name="theme-color"]');
        if (metaThemeColor) {
            metaThemeColor.setAttribute('content', isDark ? '#0f172a' : '#ffffff');
        }
    },

    init: function () {
        const isDark = this.getTheme();
        this.applyTheme(isDark);

        // Listen for system theme changes
        if (window.matchMedia) {
            window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
                if (localStorage.getItem(this.storageKey) === null) {
                    this.applyTheme(e.matches);
                }
            });
        }
    }
};

// ==========================================
// LAYOUT HELPER
// ==========================================
window.mainLayoutHelper = {
    dotNetReference: null,
    resizeHandler: null,

    checkIfMobile: function () {
        return window.innerWidth <= 991.98;
    },

    getWindowWidth: function () {
        return window.innerWidth;
    },

    initialize: function (dotNetRef) {
        this.dispose();
        this.dotNetReference = dotNetRef;
        this.attachResizeListener();
        console.log('[MainLayout] Initialized, isMobile:', this.checkIfMobile());
    },

    attachResizeListener: function () {
        let resizeTimer;
        const self = this;

        this.resizeHandler = function () {
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                if (self.dotNetReference) {
                    try {
                        const isMobile = self.checkIfMobile();
                        self.dotNetReference.invokeMethodAsync('UpdateMobileState', isMobile);
                    } catch (error) {
                        console.warn('[MainLayout] Resize callback failed:', error);
                    }
                }
            }, 150);
        };

        window.addEventListener('resize', this.resizeHandler);
    },

    dispose: function () {
        if (this.resizeHandler) {
            window.removeEventListener('resize', this.resizeHandler);
            this.resizeHandler = null;
        }
        this.dotNetReference = null;
        console.log('[MainLayout] Disposed');
    }
};

// ==========================================
// GLOBAL FUNCTIONS FOR BLAZOR
// ==========================================
window.checkIfMobile = function () {
    return window.mainLayoutHelper.checkIfMobile();
};

window.initializeMainLayout = function (dotNetRef) {
    window.mainLayoutHelper.initialize(dotNetRef);
};

window.disposeMainLayout = function () {
    window.mainLayoutHelper.dispose();
};

// Initialize theme immediately
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', function () {
        window.themeManager.init();
    });
} else {
    window.themeManager.init();
}