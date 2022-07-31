declare namespace Uno.WebAssembly.Bootstrap {
    class AotProfilerSupport {
        private _context?;
        private _unoConfig;
        constructor(context: DotnetPublicAPI, unoConfig: Uno.WebAssembly.Bootstrap.UnoConfig);
        static initialize(context: DotnetPublicAPI, unoConfig: Uno.WebAssembly.Bootstrap.UnoConfig): AotProfilerSupport;
        private attachProfilerHotKey;
        private saveAotProfile;
    }
}
declare namespace Uno.WebAssembly.Bootstrap {
    class HotReloadSupport {
        private _context?;
        constructor(context: DotnetPublicAPI);
        initializeHotReload(): Promise<void>;
    }
}
declare namespace Uno.WebAssembly.Bootstrap {
    class LogProfilerSupport {
        private _context?;
        private static _logProfilerEnabled;
        private _flushLogProfile;
        private _getLogProfilerProfileOutputFile;
        private triggerHeapShotLogProfiler;
        private _unoConfig;
        constructor(context: DotnetPublicAPI, unoConfig: Uno.WebAssembly.Bootstrap.UnoConfig);
        static initializeLogProfiler(unoConfig: Uno.WebAssembly.Bootstrap.UnoConfig): boolean;
        postInitializeLogProfiler(): void;
        private attachHotKey;
        private ensureInitializeProfilerMethods;
        private takeHeapShot;
        private readProfileFile;
        private saveLogProfile;
    }
}
declare namespace Uno.WebAssembly.Bootstrap {
    interface UnoConfig {
        uno_remote_managedpath: string;
        uno_app_base: string;
        uno_dependencies: string[];
        uno_main: string;
        assemblyFileExtension: string;
        mono_wasm_runtime: string;
        mono_wasm_runtime_size?: number;
        assemblies_with_size?: {
            [i: string]: number;
        };
        files_integrity?: {
            [i: string]: string;
        };
        total_assemblies_size?: number;
        enable_pwa: boolean;
        offline_files: string[];
        uno_shell_mode: string;
        environmentVariables?: {
            [i: string]: string;
        };
        generate_aot_profile?: boolean;
        enable_debugging?: boolean;
    }
}
declare namespace Uno.WebAssembly.Bootstrap {
    class Bootstrapper {
        disableDotnet6Compatibility: boolean;
        configSrc: string;
        onConfigLoaded: Function;
        onAbort: Function;
        onDotnetReady: Function;
        private _context?;
        private _monoConfig;
        private _unoConfig;
        private _hotReloadSupport?;
        private _logProfiler?;
        private _aotProfiler?;
        private _webAppBasePath;
        private _appBase;
        private body;
        private loader;
        private progress;
        private _isUsingCommonJS;
        private _currentBrowserIsChrome;
        private _hasReferencedPdbs;
        static ENVIRONMENT_IS_WEB: boolean;
        static ENVIRONMENT_IS_WORKER: boolean;
        static ENVIRONMENT_IS_NODE: boolean;
        static ENVIRONMENT_IS_SHELL: boolean;
        constructor(unoConfig: Uno.WebAssembly.Bootstrap.UnoConfig);
        static bootstrap(): Promise<void>;
        asDotnetConfig(): DotnetModuleConfig;
        configure(context: DotnetPublicAPI): void;
        private setupHotReload;
        private setupEmscriptenPreRun;
        private setupRequire;
        private wasmRuntimePreRun;
        private RuntimeReady;
        private configureGlobal;
        private configLoaded;
        private runtimeAbort;
        preInit(): void;
        private mainInit;
        private timezoneSetup;
        private cleanupInit;
        private initProgress;
        private reportProgressWasmLoading;
        private reportAssemblyLoading;
        private raiseLoadingError;
        private raiseLoadingWarning;
        private getFetchInit;
        private fetchWithProgress;
        private fetchFile;
        private isElectron;
        private initializeRequire;
        private require;
        private hasDebuggingEnabled;
        private attachDebuggerHotkey;
        private launchDebugger;
        private initializePWA;
    }
}
declare namespace Uno.WebAssembly.Bootstrap {
    class MonoRuntimeCompatibility {
        static load_runtime: any;
        static assembly_load: any;
        static find_class: any;
        static find_method: any;
        static invoke_method: any;
        static mono_string_get_utf8: any;
        static mono_wasm_obj_array_new: any;
        static mono_string: any;
        static mono_wasm_obj_array_set: any;
        static initialize(): void;
        static conv_string(mono_obj: any): string;
        static call_method(method: any, this_arg: any, args: any): any;
    }
}
