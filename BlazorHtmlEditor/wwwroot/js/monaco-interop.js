/**
 * Monaco Editor Interop pro Blazor
 * Poskytuje přímou integraci s Monaco Editorem bez závislosti na BlazorMonaco
 */

import { registerRazorLanguage } from './razor-monarch.js';

window.MonacoEditorInterop = {
    editors: {},
    dotnetRefs: {},

    /**
     * Vytvoří nový Monaco Editor
     */
    createEditor: async function (editorId, options, dotnetRef) {
        try {
            // Počkáme na načtení Monaco
            await this.waitForMonaco();

            // Registrujeme Razor jazyk pokud ještě není
            if (!this.razorRegistered) {
                try {
                    registerRazorLanguage();
                    this.razorRegistered = true;

                    // Pokusíme se nastavit jazyk na razor
                    options.language = 'razor';
                    console.log('Razor language registered successfully');
                } catch (error) {
                    console.warn('Could not register Razor language, using HTML:', error);
                    options.language = 'html';
                }
            } else {
                options.language = 'razor';
            }

            // Vytvoření editoru
            const editor = monaco.editor.create(document.getElementById(editorId), options);

            // Uložení reference
            this.editors[editorId] = editor;
            this.dotnetRefs[editorId] = dotnetRef;

            // Event listener pro změny obsahu
            editor.onDidChangeModelContent(() => {
                const value = editor.getValue();
                if (dotnetRef) {
                    dotnetRef.invokeMethodAsync('OnEditorContentChanged', value);
                }
            });

            console.log(`Monaco Editor created: ${editorId}`);
            return true;
        } catch (error) {
            console.error('Error creating Monaco Editor:', error);
            throw error;
        }
    },

    /**
     * Počká na načtení Monaco
     */
    waitForMonaco: function () {
        return new Promise((resolve, reject) => {
            let attempts = 0;
            const maxAttempts = 100;

            const check = () => {
                if (typeof monaco !== 'undefined') {
                    resolve();
                } else if (typeof require !== 'undefined') {
                    // Pokusíme se načíst přes require
                    require(['vs/editor/editor.main'], function () {
                        if (typeof monaco !== 'undefined') {
                            resolve();
                        } else {
                            reject(new Error('Monaco loaded but still undefined'));
                        }
                    }, function (err) {
                        if (attempts < maxAttempts) {
                            attempts++;
                            setTimeout(check, 100);
                        } else {
                            reject(new Error('Monaco failed to load: ' + err));
                        }
                    });
                } else if (attempts < maxAttempts) {
                    attempts++;
                    setTimeout(check, 100);
                } else {
                    reject(new Error('Monaco not available after ' + maxAttempts + ' attempts'));
                }
            };

            check();
        });
    },

    /**
     * Získá hodnotu z editoru
     */
    getValue: function (editorId) {
        const editor = this.editors[editorId];
        return editor ? editor.getValue() : '';
    },

    /**
     * Nastaví hodnotu editoru
     */
    setValue: function (editorId, value) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.setValue(value || '');
        }
    },

    /**
     * Vloží text na pozici kurzoru
     */
    insertText: function (editorId, text) {
        const editor = this.editors[editorId];
        if (!editor) return;

        const selection = editor.getSelection();
        const range = new monaco.Range(
            selection.startLineNumber,
            selection.startColumn,
            selection.endLineNumber,
            selection.endColumn
        );

        const op = {
            range: range,
            text: text,
            forceMoveMarkers: true
        };

        editor.executeEdits('insert-text', [op]);
        editor.focus();
    },

    /**
     * Zaměří editor
     */
    focus: function (editorId) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.focus();
        }
    },

    /**
     * Získá pozici kurzoru
     */
    getCursorPosition: function (editorId) {
        const editor = this.editors[editorId];
        if (!editor) return null;

        const position = editor.getPosition();
        return {
            lineNumber: position.lineNumber,
            column: position.column
        };
    },

    /**
     * Nastaví pozici kurzoru
     */
    setCursorPosition: function (editorId, lineNumber, column) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.setPosition({ lineNumber, column });
            editor.focus();
        }
    },

    /**
     * Získá vybraný text
     */
    getSelectedText: function (editorId) {
        const editor = this.editors[editorId];
        if (!editor) return '';

        const selection = editor.getSelection();
        return editor.getModel().getValueInRange(selection);
    },

    /**
     * Nastaví jazyk editoru
     */
    setLanguage: function (editorId, language) {
        const editor = this.editors[editorId];
        if (editor) {
            const model = editor.getModel();
            if (model) {
                monaco.editor.setModelLanguage(model, language);
            }
        }
    },

    /**
     * Změní téma editoru
     */
    setTheme: function (editorId, theme) {
        monaco.editor.setTheme(theme);
    },

    /**
     * Zničí editor a uvolní zdroje
     */
    dispose: function (editorId) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.dispose();
            delete this.editors[editorId];
            delete this.dotnetRefs[editorId];
            console.log(`Monaco Editor disposed: ${editorId}`);
        }
    },

    /**
     * Aktualizuje velikost editoru
     */
    layout: function (editorId) {
        const editor = this.editors[editorId];
        if (editor) {
            editor.layout();
        }
    }
};

console.log('MonacoEditorInterop loaded');
