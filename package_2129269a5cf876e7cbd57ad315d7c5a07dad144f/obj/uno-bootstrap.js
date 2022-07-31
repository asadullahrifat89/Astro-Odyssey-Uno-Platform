var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class AotProfilerSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                    var initializeProfile = this._context.BINDING.bind_static_method("[Uno.Wasm.AotProfiler] Uno.AotProfilerSupport:Initialize");
                    if (initializeProfile) {
                        initializeProfile();
                        this.attachProfilerHotKey();
                    }
                    else {
                        throw `Unable to find AOT Profiler initialization method`;
                    }
                }
                static initialize(context, unoConfig) {
                    if (context.Module.ENVIRONMENT_IS_WEB && unoConfig.generate_aot_profile) {
                        return new AotProfilerSupport(context, unoConfig);
                    }
                    return null;
                }
                attachProfilerHotKey() {
                    const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                    console.info(`AOT Profiler stop hotkey: Shift+${altKeyName}+P (when application has focus), or Run this.saveAotProfile() from the browser debug console.`);
                    document.addEventListener("keydown", (evt) => {
                        if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyP") {
                            this.saveAotProfile();
                        }
                    });
                }
                saveAotProfile() {
                    var stopProfile = this._context.BINDING.bind_static_method("[Uno.Wasm.AotProfiler] Uno.AotProfilerSupport:StopProfile");
                    stopProfile();
                    var a = window.document.createElement('a');
                    var blob = new Blob([this._context.Module.aot_profile_data]);
                    a.href = window.URL.createObjectURL(blob);
                    a.download = "aot.profile";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Bootstrap.AotProfilerSupport = AotProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class HotReloadSupport {
                constructor(context) {
                    this._context = context;
                }
                async initializeHotReload() {
                    (function (Blazor) {
                        Blazor._internal = {
                            initialize: function (BINDING) {
                                if (!this.getApplyUpdateCapabilitiesMethod) {
                                    this.getApplyUpdateCapabilitiesMethod = BINDING.bind_static_method("[Uno.Wasm.MetadataUpdater] Uno.Wasm.MetadataUpdate.WebAssemblyHotReload:GetApplyUpdateCapabilities");
                                    this.applyHotReloadDeltaMethod = BINDING.bind_static_method("[Uno.Wasm.MetadataUpdater] Uno.Wasm.MetadataUpdate.WebAssemblyHotReload:ApplyHotReloadDelta");
                                    this.initializeMethod = BINDING.bind_static_method("[Uno.Wasm.MetadataUpdater] Uno.Wasm.MetadataUpdate.WebAssemblyHotReload:Initialize");
                                }
                                this.initializeMethod();
                            },
                            applyExisting: async function () {
                                const webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                                var hotreloadConfigResponse = await fetch(`/_framework/unohotreload`);
                                var modifiableAssemblies = hotreloadConfigResponse.headers.get('DOTNET-MODIFIABLE-ASSEMBLIES');
                                var aspnetCoreBrowserTools = hotreloadConfigResponse.headers.get('ASPNETCORE-BROWSER-TOOLS');
                                if (modifiableAssemblies) {
                                    MONO.mono_wasm_setenv('DOTNET_MODIFIABLE_ASSEMBLIES', modifiableAssemblies);
                                }
                                {
                                    try {
                                        var m = await eval("import(`/_framework/blazor-hotreload.js`)");
                                        m.receiveHotReload();
                                    }
                                    catch (e) {
                                        console.error(`Failed to apply initial metadata delta ${e}`);
                                    }
                                }
                            },
                            getApplyUpdateCapabilities: function () {
                                this.initialize();
                                return this.getApplyUpdateCapabilitiesMethod();
                            },
                            applyHotReload: function (moduleId, metadataDelta, ilDelta) {
                                this.initialize();
                                return this.applyHotReloadDeltaMethod(moduleId, metadataDelta, ilDelta);
                            }
                        };
                    })(window.Blazor || (window.Blazor = {}));
                    window.Blazor._internal.initialize(this._context.BINDING);
                    await window.Blazor._internal.applyExisting();
                }
            }
            Bootstrap.HotReloadSupport = HotReloadSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class LogProfilerSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                }
                static initializeLogProfiler(unoConfig) {
                    const options = unoConfig.environmentVariables["UNO_BOOTSTRAP_LOG_PROFILER_OPTIONS"];
                    if (options) {
                        Module.ccall('mono_wasm_load_profiler_log', null, ['string'], [options]);
                        this._logProfilerEnabled = true;
                        return true;
                    }
                    return false;
                }
                postInitializeLogProfiler() {
                    if (LogProfilerSupport._logProfilerEnabled) {
                        this.attachHotKey();
                        setInterval(() => {
                            this.ensureInitializeProfilerMethods();
                            this._flushLogProfile();
                        }, 5000);
                    }
                }
                attachHotKey() {
                    if (this._context.Module.ENVIRONMENT_IS_WEB) {
                        if (LogProfilerSupport._logProfilerEnabled) {
                            const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                            console.info(`Log Profiler save hotkey: Shift+${altKeyName}+P (when application has focus), or Run this.saveLogProfile() from the browser debug console.`);
                            document.addEventListener("keydown", (evt) => {
                                if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyP") {
                                    this.saveLogProfile();
                                }
                            });
                            console.info(`Log Profiler take heap shot hotkey: Shift+${altKeyName}+H (when application has focus), or Run this.takeHeapShot() from the browser debug console.`);
                            document.addEventListener("keydown", (evt) => {
                                if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyH") {
                                    this.takeHeapShot();
                                }
                            });
                        }
                    }
                }
                ensureInitializeProfilerMethods() {
                    if (LogProfilerSupport._logProfilerEnabled && !this._flushLogProfile) {
                        this._flushLogProfile = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:FlushProfile");
                        this._getLogProfilerProfileOutputFile = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:GetProfilerProfileOutputFile");
                        this.triggerHeapShotLogProfiler = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:TriggerHeapShot");
                    }
                }
                takeHeapShot() {
                    this.ensureInitializeProfilerMethods();
                    this.triggerHeapShotLogProfiler();
                }
                readProfileFile() {
                    this.ensureInitializeProfilerMethods();
                    this._flushLogProfile();
                    var profileFilePath = this._getLogProfilerProfileOutputFile();
                    var stat = FS.stat(profileFilePath);
                    if (stat && stat.size > 0) {
                        return FS.readFile(profileFilePath);
                    }
                    else {
                        return null;
                    }
                }
                saveLogProfile() {
                    this.ensureInitializeProfilerMethods();
                    var profileArray = this.readProfileFile();
                    var a = window.document.createElement('a');
                    a.href = window.URL.createObjectURL(new Blob([profileArray]));
                    a.download = "profile.mlpd";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Bootstrap.LogProfilerSupport = LogProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class Bootstrapper {
                constructor(unoConfig) {
                    this._unoConfig = unoConfig;
                    this._webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                    this._appBase = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_APP_BASE"];
                    this.disableDotnet6Compatibility = false;
                    this.configSrc = `${this._webAppBasePath}${this._appBase}/mono-config.json`;
                    this.onConfigLoaded = () => this.configLoaded();
                    this.onDotnetReady = () => this.RuntimeReady();
                    this.onAbort = () => this.runtimeAbort();
                }
                static async bootstrap() {
                    try {
                        Bootstrapper.ENVIRONMENT_IS_WEB = typeof window === 'object';
                        Bootstrapper.ENVIRONMENT_IS_WORKER = typeof globalThis.importScripts === 'function';
                        Bootstrapper.ENVIRONMENT_IS_NODE = typeof globalThis.process === 'object' && typeof globalThis.process.versions === 'object' && typeof globalThis.process.versions.node === 'string';
                        Bootstrapper.ENVIRONMENT_IS_SHELL = !Bootstrapper.ENVIRONMENT_IS_WEB && !Bootstrapper.ENVIRONMENT_IS_NODE && !Bootstrapper.ENVIRONMENT_IS_WORKER;
                        let runtime = null;
                        let DOMContentLoaded = false;
                        if (typeof window === 'object') {
                            globalThis.document.addEventListener("DOMContentLoaded", () => {
                                DOMContentLoaded = true;
                                runtime?.preInit();
                            });
                        }
                        var config = await eval("import('./uno-config.js')");
                        runtime = new Bootstrapper(config.config);
                        if (DOMContentLoaded) {
                            runtime.preInit();
                        }
                        var m = await eval("import(`./dotnet.js`)");
                        const { MONO, BINDING } = await m.default((context) => {
                            runtime.configure(context);
                            return runtime.asDotnetConfig();
                        });
                    }
                    catch (e) {
                        throw `.NET runtime initialization failed (${e})`;
                    }
                }
                asDotnetConfig() {
                    return {
                        disableDotnet6Compatibility: this.disableDotnet6Compatibility,
                        configSrc: this.configSrc,
                        baseUrl: this._unoConfig.uno_app_base + "/",
                        mainScriptPath: "dotnet.js",
                        onConfigLoaded: this.onConfigLoaded,
                        onDotnetReady: this.onDotnetReady,
                        onAbort: this.onAbort,
                        exports: ["IDBFS", "FS"]
                    };
                }
                configure(context) {
                    this._context = context;
                    this._context.Module.ENVIRONMENT_IS_WEB = Bootstrapper.ENVIRONMENT_IS_WEB;
                    this._context.Module.ENVIRONMENT_IS_NODE = Bootstrapper.ENVIRONMENT_IS_NODE;
                    this.setupRequire();
                    this.setupEmscriptenPreRun();
                    this.setupHotReload();
                }
                setupHotReload() {
                    if (this._context.Module.ENVIRONMENT_IS_WEB && this.hasDebuggingEnabled()) {
                        this._hotReloadSupport = new Bootstrap.HotReloadSupport(this._context);
                    }
                }
                setupEmscriptenPreRun() {
                    if (!this._context.Module.preRun) {
                        this._context.Module.preRun = [];
                    }
                    else if (typeof this._context.Module.preInit === "function") {
                        this._context.Module.preRun = [];
                    }
                    this._context.Module.preRun.push(() => this.wasmRuntimePreRun());
                }
                setupRequire() {
                    const anyModule = this._context.Module;
                    anyModule.imports = anyModule.imports || {};
                    anyModule.imports.require = globalThis.require;
                }
                wasmRuntimePreRun() {
                    if (this._unoConfig.environmentVariables) {
                        for (let key in this._unoConfig.environmentVariables) {
                            if (this._unoConfig.environmentVariables.hasOwnProperty(key)) {
                                if (this._monoConfig.enable_debugging)
                                    console.log(`Setting ${key}=${this._unoConfig.environmentVariables[key]}`);
                                this._monoConfig.environment_variables[key] = this._unoConfig.environmentVariables[key];
                            }
                        }
                    }
                    if (Bootstrap.LogProfilerSupport.initializeLogProfiler(this._unoConfig)) {
                        this._logProfiler = new Bootstrap.LogProfilerSupport(this._context, this._unoConfig);
                    }
                }
                RuntimeReady() {
                    Bootstrap.MonoRuntimeCompatibility.initialize();
                    this.configureGlobal();
                    this.initializeRequire();
                    this._aotProfiler = Bootstrap.AotProfilerSupport.initialize(this._context, this._unoConfig);
                    this._logProfiler?.postInitializeLogProfiler();
                }
                configureGlobal() {
                    var thatGlobal = globalThis;
                    thatGlobal.config = this._unoConfig;
                    thatGlobal.MonoRuntime = Bootstrap.MonoRuntimeCompatibility;
                    thatGlobal.lengthBytesUTF8 = this._context.Module.lengthBytesUTF8;
                    thatGlobal.stringToUTF8 = this._context.Module.stringToUTF8;
                    thatGlobal.UTF8ToString = this._context.Module.UTF8ToString;
                    thatGlobal.UTF8ArrayToString = this._context.Module.UTF8ArrayToString;
                }
                configLoaded() {
                    this._monoConfig = this._context.MONO.config;
                    if (this._unoConfig.generate_aot_profile) {
                        this._monoConfig.aot_profiler_options = {
                            write_at: "Uno.AotProfilerSupport::StopProfile",
                            send_to: "Uno.AotProfilerSupport::DumpAotProfileData"
                        };
                    }
                    if (this._monoConfig != null) {
                        this._monoConfig.fetch_file_cb = (asset) => this.fetchFile(asset);
                    }
                    else {
                        throw `Invalid .NET onfiguration`;
                    }
                }
                runtimeAbort() {
                }
                preInit() {
                    this.body = document.getElementById("uno-body");
                    this.initProgress();
                }
                async mainInit() {
                    try {
                        this.attachDebuggerHotkey();
                        this.timezoneSetup();
                        if (this._hotReloadSupport) {
                            await this._hotReloadSupport.initializeHotReload();
                        }
                        this._context.MONO.mono_run_main(this._unoConfig.uno_main, []);
                        this.initializePWA();
                    }
                    catch (e) {
                        console.error(e);
                    }
                    this.cleanupInit();
                }
                timezoneSetup() {
                    var timeZoneSetupMethod = this._context.BINDING.bind_static_method("[Uno.Wasm.TimezoneData] Uno.Wasm.TimezoneData.TimezoneHelper:Setup");
                    if (timeZoneSetupMethod) {
                        timeZoneSetupMethod(Intl.DateTimeFormat().resolvedOptions().timeZone);
                    }
                }
                cleanupInit() {
                    if (this.loader && this.loader.parentNode) {
                        this.loader.parentNode.removeChild(this.loader);
                    }
                }
                initProgress() {
                    this.loader = this.body.querySelector(".uno-loader");
                    if (this.loader) {
                        const totalBytesToDownload = this._unoConfig.mono_wasm_runtime_size + this._unoConfig.total_assemblies_size;
                        const progress = this.loader.querySelector("progress");
                        progress.max = totalBytesToDownload;
                        progress.value = "";
                        this.progress = progress;
                    }
                    const configLoader = () => {
                        if (manifest && manifest.lightThemeBackgroundColor) {
                            this.loader.style.setProperty("--light-theme-bg-color", manifest.lightThemeBackgroundColor);
                        }
                        if (manifest && manifest.darkThemeBackgroundColor) {
                            this.loader.style.setProperty("--dark-theme-bg-color", manifest.darkThemeBackgroundColor);
                        }
                        if (manifest && manifest.splashScreenColor && manifest.splashScreenColor != "transparent") {
                            this.loader.style.setProperty("background-color", manifest.splashScreenColor);
                        }
                        if (manifest && manifest.accentColor) {
                            this.loader.style.setProperty("--accent-color", manifest.accentColor);
                        }
                        const img = this.loader.querySelector("img");
                        if (manifest && manifest.splashScreenImage) {
                            if (!manifest.splashScreenImage.match(/^(http(s)?:\/\/.)/g)) {
                                manifest.splashScreenImage = `${this._unoConfig.uno_app_base}/${manifest.splashScreenImage}`;
                            }
                            img.setAttribute("src", manifest.splashScreenImage);
                        }
                        else {
                            img.setAttribute("src", "https://nv-assets.azurewebsites.net/logos/uno-splashscreen-light.png");
                        }
                    };
                    let manifest = window["UnoAppManifest"];
                    if (manifest) {
                        configLoader();
                    }
                    else {
                        for (var i = 0; i < this._unoConfig.uno_dependencies.length; i++) {
                            if (this._unoConfig.uno_dependencies[i].endsWith('AppManifest')
                                || this._unoConfig.uno_dependencies[i].endsWith('AppManifest.js')) {
                                require([this._unoConfig.uno_dependencies[i]], function () {
                                    manifest = window["UnoAppManifest"];
                                    configLoader();
                                });
                                break;
                            }
                        }
                    }
                }
                reportProgressWasmLoading(loaded) {
                    if (this.progress) {
                        this.progress.value = loaded;
                    }
                }
                reportAssemblyLoading(adding) {
                    if (this.progress) {
                        this.progress.value += adding;
                    }
                }
                raiseLoadingError(err) {
                    this.loader.setAttribute("loading-alert", "error");
                    const alert = this.loader.querySelector(".alert");
                    let title = alert.getAttribute("title");
                    if (title) {
                        title += `\n${err}`;
                    }
                    else {
                        title = `${err}`;
                    }
                    alert.setAttribute("title", title);
                }
                raiseLoadingWarning(msg) {
                    if (this.loader.getAttribute("loading-alert") !== "error") {
                        this.loader.setAttribute("loading-alert", "warning");
                    }
                    const alert = this.loader.querySelector(".alert");
                    let title = alert.getAttribute("title");
                    if (title) {
                        title += `\n${msg}`;
                    }
                    else {
                        title = `${msg}`;
                    }
                    alert.setAttribute("title", title);
                }
                getFetchInit(url) {
                    const fileName = url.substring(url.lastIndexOf("/") + 1);
                    const init = { credentials: "omit" };
                    if (this._unoConfig.files_integrity.hasOwnProperty(fileName)) {
                        init.integrity = this._unoConfig.files_integrity[fileName];
                    }
                    return init;
                }
                fetchWithProgress(url, progressCallback) {
                    if (!this.loader) {
                        return fetch(url, this.getFetchInit(url));
                    }
                    return fetch(url, this.getFetchInit(url))
                        .then(response => {
                        if (!response.ok) {
                            throw Error(`${response.status} ${response.statusText}`);
                        }
                        try {
                            let loaded = 0;
                            const stream = new ReadableStream({
                                start(ctl) {
                                    const reader = response.body.getReader();
                                    read();
                                    function read() {
                                        reader.read()
                                            .then(({ done, value }) => {
                                            if (done) {
                                                ctl.close();
                                                return;
                                            }
                                            loaded += value.byteLength;
                                            progressCallback(loaded, value.byteLength);
                                            ctl.enqueue(value);
                                            read();
                                        })
                                            .catch(error => {
                                            console.error(error);
                                            ctl.error(error);
                                        });
                                    }
                                }
                            });
                            return new Response(stream, response);
                        }
                        catch (ex) {
                            return response;
                        }
                    })
                        .catch(err => this.raiseLoadingError(err));
                }
                fetchFile(asset) {
                    if (asset.lastIndexOf(".dll") !== -1) {
                        asset = asset.replace(".dll", `.${this._unoConfig.assemblyFileExtension}`);
                    }
                    if (asset.startsWith("icudt") && asset.endsWith(".dat")) {
                        asset = `${this._unoConfig.uno_app_base}/${asset}`;
                    }
                    asset = asset.replace("/managed/", `/${this._unoConfig.uno_remote_managedpath}/`);
                    if (this._context.Module.ENVIRONMENT_IS_NODE) {
                        var fs = require('fs');
                        console.log('Loading... ' + asset);
                        var binary = fs.readFileSync(asset);
                        var resolve_func2 = function (resolve, reject) {
                            resolve(new Uint8Array(binary));
                        };
                        var resolve_func1 = function (resolve, reject) {
                            var response = {
                                ok: true,
                                url: asset,
                                arrayBuffer: function () {
                                    return new Promise(resolve_func2);
                                }
                            };
                            resolve(response);
                        };
                        return new Promise(resolve_func1);
                    }
                    else {
                        if (!this._unoConfig.enable_debugging) {
                            const assemblyName = asset.substring(asset.lastIndexOf("/") + 1);
                            if (this._unoConfig.assemblies_with_size.hasOwnProperty(assemblyName)) {
                                return this
                                    .fetchWithProgress(asset, (loaded, adding) => this.reportAssemblyLoading(adding));
                            }
                        }
                        return fetch(asset);
                    }
                }
                isElectron() {
                    return navigator.userAgent.indexOf('Electron') !== -1;
                }
                initializeRequire() {
                    this._isUsingCommonJS = this._unoConfig.uno_shell_mode !== "BrowserEmbedded" && (this._context.Module.ENVIRONMENT_IS_NODE || this.isElectron());
                    if (this._unoConfig.enable_debugging)
                        console.log("Done loading the BCL");
                    if (this._unoConfig.uno_dependencies && this._unoConfig.uno_dependencies.length !== 0) {
                        let pending = this._unoConfig.uno_dependencies.length;
                        const checkDone = (dependency) => {
                            --pending;
                            if (this._unoConfig.enable_debugging)
                                console.log(`Loaded dependency (${dependency}) - remains ${pending} other(s).`);
                            if (pending === 0) {
                                this.mainInit();
                            }
                        };
                        this._unoConfig.uno_dependencies.forEach((dependency) => {
                            if (this._unoConfig.enable_debugging)
                                console.log(`Loading dependency (${dependency})`);
                            let processDependency = (instance) => {
                                if (instance && instance.HEAP8 !== undefined) {
                                    const existingInitializer = instance.onRuntimeInitialized;
                                    if (this._unoConfig.enable_debugging)
                                        console.log(`Waiting for dependency (${dependency}) initialization`);
                                    instance.onRuntimeInitialized = () => {
                                        checkDone(dependency);
                                        if (existingInitializer)
                                            existingInitializer();
                                    };
                                }
                                else {
                                    checkDone(dependency);
                                }
                            };
                            this.require([dependency], processDependency);
                        });
                    }
                    else {
                        this.mainInit();
                    }
                }
                require(modules, callback) {
                    if (this._isUsingCommonJS) {
                        modules.forEach(id => {
                            setTimeout(() => {
                                const d = require('./' + id);
                                callback(d);
                            }, 0);
                        });
                    }
                    else {
                        if (typeof require === undefined) {
                            throw `Require.js has not been loaded yet. If you have customized your index.html file, make sure that <script src="./require.js"></script> does not contain the defer directive.`;
                        }
                        require(modules, callback);
                    }
                }
                hasDebuggingEnabled() {
                    return this._hasReferencedPdbs && this._currentBrowserIsChrome;
                }
                attachDebuggerHotkey() {
                    if (this._context.Module.ENVIRONMENT_IS_WEB) {
                        let loadAssemblyUrls = this._monoConfig.assets.map(a => a.name);
                        this._currentBrowserIsChrome = window.chrome
                            && navigator.userAgent.indexOf("Edge") < 0;
                        this._hasReferencedPdbs = loadAssemblyUrls
                            .some(function (url) { return /\.pdb$/.test(url); });
                        const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                        if (this.hasDebuggingEnabled()) {
                            console.info(`Debugging hotkey: Shift+${altKeyName}+D (when application has focus)`);
                        }
                        document.addEventListener("keydown", (evt) => {
                            if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyD") {
                                if (!this._hasReferencedPdbs) {
                                    console.error("Cannot start debugging, because the application was not compiled with debugging enabled.");
                                }
                                else if (!this._currentBrowserIsChrome) {
                                    console.error("Currently, only Chrome is supported for debugging.");
                                }
                                else {
                                    this.launchDebugger();
                                }
                            }
                        });
                    }
                }
                launchDebugger() {
                    const link = document.createElement("a");
                    link.href = `_framework/debug?url=${encodeURIComponent(location.href)}`;
                    link.target = "_blank";
                    link.rel = "noopener noreferrer";
                    link.click();
                }
                initializePWA() {
                    if (typeof window === 'object') {
                        if (this._unoConfig.enable_pwa && 'serviceWorker' in navigator) {
                            if (navigator.serviceWorker.controller) {
                                console.debug("Active service worker found, skipping register");
                            }
                            else {
                                const _webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                                console.debug(`Registering service worker for ${_webAppBasePath}`);
                                navigator.serviceWorker
                                    .register(`${_webAppBasePath}service-worker.js`, {
                                    scope: _webAppBasePath
                                })
                                    .then(function () {
                                    console.debug('Service Worker Registered');
                                });
                            }
                        }
                    }
                }
            }
            Bootstrap.Bootstrapper = Bootstrapper;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
Uno.WebAssembly.Bootstrap.Bootstrapper.bootstrap();
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class MonoRuntimeCompatibility {
                static initialize() {
                    MonoRuntimeCompatibility.load_runtime = Module.cwrap("mono_wasm_load_runtime", null, ["string", "number"]);
                    MonoRuntimeCompatibility.assembly_load = Module.cwrap("mono_wasm_assembly_load", "number", ["string"]);
                    MonoRuntimeCompatibility.find_class = Module.cwrap("mono_wasm_assembly_find_class", "number", ["number", "string", "string"]);
                    MonoRuntimeCompatibility.find_method = Module.cwrap("mono_wasm_assembly_find_method", "number", ["number", "string", "number"]);
                    MonoRuntimeCompatibility.invoke_method = Module.cwrap("mono_wasm_invoke_method", "number", ["number", "number", "number"]);
                    MonoRuntimeCompatibility.mono_string_get_utf8 = Module.cwrap("mono_wasm_string_get_utf8", "number", ["number"]);
                    MonoRuntimeCompatibility.mono_string = Module.cwrap("mono_wasm_string_from_js", "number", ["string"]);
                    MonoRuntimeCompatibility.mono_wasm_obj_array_new = Module.cwrap("mono_wasm_obj_array_new", "number", ["number"]);
                    MonoRuntimeCompatibility.mono_wasm_obj_array_set = Module.cwrap("mono_wasm_obj_array_set", null, ["number", "number", "number"]);
                }
                static conv_string(mono_obj) {
                    if (mono_obj === 0)
                        return null;
                    const raw = MonoRuntimeCompatibility.mono_string_get_utf8(mono_obj);
                    const res = Module.UTF8ToString(raw);
                    Module._free(raw);
                    return res;
                }
                static call_method(method, this_arg, args) {
                    const args_mem = Module._malloc(args.length * 4);
                    const eh_throw = Module._malloc(4);
                    for (let i = 0; i < args.length; ++i)
                        Module.setValue(args_mem + i * 4, args[i], "i32");
                    Module.setValue(eh_throw, 0, "i32");
                    const res = MonoRuntimeCompatibility.invoke_method(method, this_arg, args_mem, eh_throw);
                    const eh_res = Module.getValue(eh_throw, "i32");
                    Module._free(args_mem);
                    Module._free(eh_throw);
                    if (eh_res !== 0) {
                        const msg = MonoRuntimeCompatibility.conv_string(res);
                        throw new Error(msg);
                    }
                    return res;
                }
            }
            Bootstrap.MonoRuntimeCompatibility = MonoRuntimeCompatibility;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
