/**
 * GrapesJS loader a helper funkce pro vizuální editor Razor šablon
 */

window.GrapesJSHelper = {
    editors: {},

    /**
     * Inicializuje GrapesJS editor
     */
    async initialize(containerId, initialHtml, initialCss, dotnetRef) {
        try {
            // Počkáme na načtení GrapesJS
            await this.waitForGrapesJS();

            console.log('Initializing GrapesJS with CSS:', initialCss ? initialCss.length + ' chars' : 'none');

            // Vytvoření editoru - čistý editor bez jakéhokoli UI
            const editor = window.grapesjs.init({
                container: `#${containerId}`,
                height: '100%',
                width: 'auto',
                fromElement: false,
                storageManager: false,
                noticeOnUnload: false,

                // Vypnout všechny managery
                blockManager: false,
                layerManager: false,
                traitManager: false,
                selectorManager: false,
                styleManager: false,

                // Vypnout panely
                panels: { defaults: [] },

                // Žádné device managery
                deviceManager: { devices: [] },

                // Rich Text Editor - povolit a nastavit
                richTextEditor: {
                    enable: true,
                    actions: ['bold', 'italic', 'underline', 'strikethrough', 'link']
                },

                // Canvas - povolit klikání
                canvas: {
                    scripts: [],
                    styles: []
                },
            });

            // Agresivně odstranit všechny panely z DOM
            setTimeout(() => {
                const unwantedElements = [
                    '.gjs-pn-panels',
                    '.gjs-pn-views-container',
                    '.gjs-pn-views',
                    '.gjs-pn-commands',
                    '.gjs-pn-options',
                    '.gjs-pn-devices-c',
                    '[data-gjs-type="Panels"]',
                    '.gjs-blocks-c',
                    '.gjs-sm-sectors',
                    '.gjs-layers',
                    '.gjs-traits-c'
                ];

                unwantedElements.forEach(selector => {
                    const elements = document.querySelectorAll(selector);
                    elements.forEach(el => {
                        console.log('Removing element:', selector);
                        el.remove();
                    });
                });

                console.log('Panels cleanup complete');
            }, 100);

            // Skrýt všechny defaultní UI elementy pomocí CSS
            const style = document.createElement('style');
            style.textContent = `
                /* Agresivně skrýt VŠECHNY panely */
                .gjs-pn-panels,
                .gjs-pn-views-container,
                .gjs-pn-views,
                .gjs-pn-buttons,
                .gjs-pn-panel,
                .gjs-pn-commands,
                .gjs-pn-options,
                .gjs-pn-devices-c,
                .gjs-blocks-c,
                .gjs-sm-sectors,
                .gjs-layers,
                .gjs-traits-c,
                .gjs-toolbar,
                .gjs-resizer-h,
                .gjs-resizer-v,
                .gjs-badge,
                .gjs-toolbar-items,
                [data-gjs-type="Panels"] {
                    display: none !important;
                    visibility: hidden !important;
                    width: 0 !important;
                    height: 0 !important;
                    overflow: hidden !important;
                }

                /* Toolbar zobrazit POUZE při editaci textu */
                .gjs-rte-toolbar {
                    display: flex !important;
                    visibility: visible !important;
                    background: white;
                    border: 1px solid #ddd;
                    padding: 5px;
                    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
                }

                /* Highlighted element - modrý okraj */
                .gjs-selected {
                    outline: 2px solid #3b97e3 !important;
                    outline-offset: -2px;
                }

                /* Hover effect */
                [data-gjs-type]:hover {
                    outline: 1px dashed #3b97e3;
                    outline-offset: -1px;
                    cursor: pointer;
                }

                /* Canvas plná šířka - důležité! */
                .gjs-cv-canvas {
                    width: 100% !important;
                    height: 100% !important;
                    top: 0 !important;
                    left: 0 !important;
                    right: 0 !important;
                    bottom: 0 !important;
                }

                /* Frame uvnitř canvasu */
                .gjs-frame {
                    width: 100% !important;
                    height: 100% !important;
                }

                /* Editable text - cursor */
                [contenteditable="true"] {
                    cursor: text !important;
                }
            `;
            document.head.appendChild(style);

            // Nastavení počátečního obsahu
            if (initialHtml) {
                editor.setComponents(initialHtml);
            }

            // Nastavení CSS do editoru
            if (initialCss) {
                console.log('Adding CSS to editor:', initialCss.substring(0, 100) + '...');
                editor.setStyle(initialCss);
            }

            // Nastavit všechny komponenty jako editovatelné při jejich vytvoření
            editor.on('component:add', (component) => {
                const tagName = component.get('tagName');
                const textTags = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'span', 'div', 'a', 'li', 'td', 'th'];

                if (textTags.includes(tagName?.toLowerCase())) {
                    component.set('editable', true);
                    component.set('draggable', true);
                }
            });

            // Povolit editaci všech komponent při výběru
            editor.on('component:selected', (component) => {
                console.log('Component selected:', component.get('tagName'));

                // Povolit editaci a přesouvání
                component.set('draggable', true);

                // Pro text komponenty povolit RTE
                const tagName = component.get('tagName');
                const textTags = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'span', 'div', 'a', 'li', 'td', 'th'];
                if (textTags.includes(tagName?.toLowerCase())) {
                    component.set('editable', true);
                    console.log('Text component selected - double-click to edit');
                }
            });

            // Aktivovat RTE při double-click - vylepšená verze
            editor.on('component:dblclick', (component) => {
                console.log('Double-click detected on:', component.get('tagName'));

                const tagName = component.get('tagName');
                const textTags = ['p', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'span', 'div', 'a', 'li', 'td', 'th'];

                // Zkontrolovat, že je to textová komponenta
                if (!textTags.includes(tagName?.toLowerCase())) {
                    console.log('Not a text component, skipping RTE activation');
                    return;
                }

                // Nastavit komponentu jako editovatelnou
                component.set('editable', true);

                // Získat Rich Text Editor
                const rte = editor.RichTextEditor;
                if (!rte) {
                    console.error('RichTextEditor not available');
                    return;
                }

                try {
                    // Vybrat komponentu
                    editor.select(component);

                    // Získat DOM element
                    const el = component.getEl();
                    if (!el) {
                        console.error('Component element not found');
                        return;
                    }

                    console.log('Activating RTE on element:', el);

                    // Aktivovat RTE s malým zpožděním pro jistotu
                    setTimeout(() => {
                        rte.enable(el);

                        // Focus element pro okamžitou editaci
                        el.focus();

                        console.log('RTE activated successfully');
                    }, 50);
                } catch (error) {
                    console.error('Error activating RTE:', error);
                }
            });

            // Event listener pro změny
            editor.on('component:update', () => {
                if (dotnetRef) {
                    const html = editor.getHtml();
                    const css = editor.getCss();
                    dotnetRef.invokeMethodAsync('OnContentChanged', html, css);
                }
            });

            // Log když začne/skončí editace
            editor.on('rte:enable', () => {
                console.log('Text editing started');
            });

            editor.on('rte:disable', () => {
                console.log('Text editing finished');
            });

            // Registrace custom komponent pro Razor placeholdery
            this.registerRazorComponents(editor);

            // Uložení reference
            this.editors[containerId] = editor;

            console.log('GrapesJS editor initialized successfully:', containerId);
            console.log('- Double-click text to edit');
            console.log('- Click element to select');
            console.log('- Drag to move elements');
            return true;

        } catch (error) {
            console.error('Error initializing GrapesJS:', error);
            return false;
        }
    },

    /**
     * Počká na načtení GrapesJS nebo ho načte z CDN
     */
    async waitForGrapesJS() {
        return new Promise((resolve, reject) => {
            // Pokud už je načten, okamžitě resolve
            if (typeof window.grapesjs !== 'undefined') {
                console.log('GrapesJS already loaded');
                resolve();
                return;
            }

            console.log('GrapesJS not found, loading from CDN...');

            // Načteme CSS pokud neexistuje
            if (!document.querySelector('link[href*="grapesjs"]')) {
                const css = document.createElement('link');
                css.rel = 'stylesheet';
                css.href = 'https://unpkg.com/grapesjs@0.21.10/dist/css/grapes.min.css';
                document.head.appendChild(css);
                console.log('GrapesJS CSS added (version 0.21.10)');
            }

            // Najdeme existující script tag
            const existingScript = document.querySelector('script[src*="grapesjs"]');

            if (existingScript) {
                console.log('GrapesJS script tag found, checking if loaded...');

                // Počkáme chvíli jestli se náhodou nenačte
                let attempts = 0;
                const quickCheck = () => {
                    if (typeof window.grapesjs !== 'undefined') {
                        console.log('GrapesJS is ready (from existing script)');
                        resolve();
                    } else if (attempts < 20) { // Jen 2 sekundy počkáme
                        attempts++;
                        setTimeout(quickCheck, 100);
                    } else {
                        // Existující script nefunguje, odstraníme ho a načteme nový
                        console.warn('Existing GrapesJS script did not load, removing and loading fresh...');
                        existingScript.remove();
                        this.loadFreshGrapesJS(resolve, reject);
                    }
                };
                quickCheck();
            } else {
                // Žádný script tag neexistuje, načteme nový
                this.loadFreshGrapesJS(resolve, reject);
            }
        });
    },

    /**
     * Načte fresh GrapesJS script z CDN
     */
    loadFreshGrapesJS(resolve, reject) {
        // Temporarily disable AMD/RequireJS to force GrapesJS to load as a global
        // GrapesJS uses UMD and will register as AMD module if define() exists
        const oldDefine = window.define;
        const oldRequire = window.require;

        console.log('Temporarily disabling AMD loader to force global GrapesJS');
        window.define = undefined;
        window.require = undefined;

        const script = document.createElement('script');
        script.src = 'https://unpkg.com/grapesjs@0.21.10/dist/grapes.min.js';

        console.log(`Loading GrapesJS from: ${script.src}`);

        script.onload = () => {
            console.log('GrapesJS script loaded successfully');

            // Restore AMD loader
            if (oldDefine) {
                window.define = oldDefine;
                console.log('AMD loader restored');
            }
            if (oldRequire) {
                window.require = oldRequire;
            }

            // Check immediately if global is available
            if (typeof window.grapesjs !== 'undefined') {
                console.log('GrapesJS global is available!');
                resolve();
                return;
            }

            // Wait a bit for the global to become available
            let attempts = 0;
            const checkGlobal = () => {
                if (typeof window.grapesjs !== 'undefined') {
                    console.log('GrapesJS is ready');
                    resolve();
                } else if (attempts < 10) {
                    attempts++;
                    setTimeout(checkGlobal, 100);
                } else {
                    console.error('GrapesJS failed to load as global. window.grapesjs:', typeof window.grapesjs);
                    reject(new Error('GrapesJS script loaded but global not available'));
                }
            };
            checkGlobal();
        };

        script.onerror = (error) => {
            // Restore AMD loader on error too
            if (oldDefine) window.define = oldDefine;
            if (oldRequire) window.require = oldRequire;

            console.error('Failed to load GrapesJS from CDN:', error);
            reject(new Error('Failed to load GrapesJS from CDN'));
        };

        document.head.appendChild(script);
    },

    /**
     * Registruje custom komponenty pro Razor placeholdery
     */
    registerRazorComponents(editor) {
        editor.DomComponents.addType('razor-placeholder', {
            model: {
                defaults: {
                    tagName: 'span',
                    attributes: { class: 'razor-placeholder' },
                    editable: true,
                    droppable: false,
                    traits: [
                        {
                            type: 'text',
                            label: 'Razor Expression',
                            name: 'data-razor',
                        },
                    ],
                },
            },
            view: {
                onRender() {
                    const razorExpr = this.model.getAttributes()['data-razor'];
                    if (razorExpr) {
                        this.el.style.backgroundColor = '#e0e0e0';
                        this.el.style.padding = '2px 8px';
                        this.el.style.borderRadius = '3px';
                        this.el.style.display = 'inline-block';
                        this.el.style.fontFamily = 'monospace';
                        this.el.style.fontSize = '0.9em';
                    }
                },
            },
        });
    },

    /**
     * Získá HTML obsah z editoru
     */
    getHtml(containerId) {
        const editor = this.editors[containerId];
        return editor ? editor.getHtml() : '';
    },

    /**
     * Získá CSS z editoru
     */
    getCss(containerId) {
        const editor = this.editors[containerId];
        return editor ? editor.getCss() : '';
    },

    /**
     * Nastaví HTML obsah
     */
    setHtml(containerId, html) {
        const editor = this.editors[containerId];
        if (editor) {
            editor.setComponents(html);
        }
    },

    /**
     * Vloží Razor placeholder
     */
    insertRazorPlaceholder(containerId, razorExpression) {
        console.log('=== insertRazorPlaceholder called ===');
        console.log('Container ID:', containerId);
        console.log('Razor Expression:', razorExpression);

        const editor = this.editors[containerId];
        if (!editor) {
            console.error('Editor not found for container:', containerId);
            return false;
        }

        try {
            const selected = editor.getSelected();
            console.log('Currently selected component:', selected ? selected.get('tagName') : 'none');

            // Vytvořit novou komponentu
            const newComponent = {
                tagName: 'span',
                type: 'text',
                content: razorExpression,
                editable: true,
                draggable: true,
                droppable: false,
                attributes: {
                    'class': 'razor-placeholder',
                    'data-razor': razorExpression,
                },
                style: {
                    'background-color': '#e0e0e0',
                    'padding': '2px 8px',
                    'border-radius': '3px',
                    'display': 'inline-block',
                    'font-family': 'monospace',
                    'font-size': '0.9em',
                    'margin': '0 2px',
                }
            };

            let addedComponent = null;

            // Pokud je něco vybrané, přidat do vybrané komponenty
            if (selected) {
                console.log('Appending to selected component:', selected.get('tagName'));
                addedComponent = selected.append(newComponent)[0];
            } else {
                // Jinak přidat do root wrapperu
                console.log('No selection - adding to root wrapper');
                const wrapper = editor.getWrapper();
                if (wrapper) {
                    addedComponent = wrapper.append(newComponent)[0];
                } else {
                    console.log('No wrapper - using addComponents');
                    const components = editor.addComponents(newComponent);
                    addedComponent = Array.isArray(components) ? components[0] : components;
                }
            }

            // Vybrat nově přidanou komponentu pro vizuální feedback
            if (addedComponent) {
                console.log('New component added successfully');
                editor.select(addedComponent);

                // Trigger update event pro Blazor callback
                setTimeout(() => {
                    const html = editor.getHtml();
                    const css = editor.getCss();
                    console.log('Triggering content update after insertion');
                    console.log('HTML length:', html.length);

                    // Manually trigger update event
                    editor.trigger('component:update');
                }, 100);
            } else {
                console.warn('Component was not added properly');
            }

            console.log('=== Razor placeholder inserted successfully ===');
            return true;

        } catch (error) {
            console.error('Error inserting Razor placeholder:', error);
            return false;
        }
    },

    /**
     * Zničí editor
     */
    destroy(containerId) {
        const editor = this.editors[containerId];
        if (editor) {
            editor.destroy();
            delete this.editors[containerId];
        }
    }
};

console.log('GrapesJS loader ready');
