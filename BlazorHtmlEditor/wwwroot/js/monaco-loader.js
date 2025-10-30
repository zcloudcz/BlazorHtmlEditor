/**
 * Monaco Editor loader a helper funkce pro Blazor
 */

import { registerRazorLanguage } from './razor-monarch.js';

window.MonacoEditorHelper = {
    /**
     * Inicializuje Monaco editor s Razor podporou
     */
    async initialize() {
        // Počkáme na načtení Monaco přes require.js
        return new Promise((resolve, reject) => {
            let attempts = 0;
            const maxAttempts = 100;

            const checkMonaco = () => {
                if (typeof monaco !== 'undefined') {
                    try {
                        registerRazorLanguage();
                        console.log('Monaco Editor initialized with Razor support');
                        resolve(true);
                    } catch (error) {
                        console.error('Error registering Razor language:', error);
                        resolve(true); // Stále pokračujeme i když selže registrace jazyka
                    }
                } else if (typeof require !== 'undefined') {
                    // Pokusíme se načíst Monaco přes require
                    require(['vs/editor/editor.main'], function() {
                        if (typeof monaco !== 'undefined') {
                            try {
                                registerRazorLanguage();
                                console.log('Monaco Editor loaded via require and Razor support added');
                                resolve(true);
                            } catch (error) {
                                console.error('Error registering Razor language:', error);
                                resolve(true);
                            }
                        } else {
                            reject(new Error('Monaco failed to load via require'));
                        }
                    }, function(err) {
                        console.error('Error loading Monaco via require:', err);
                        // Zkusíme počkat a zkontrolovat znovu
                        if (attempts < maxAttempts) {
                            attempts++;
                            setTimeout(checkMonaco, 100);
                        } else {
                            reject(new Error('Monaco Editor failed to load after ' + maxAttempts + ' attempts'));
                        }
                    });
                } else if (attempts < maxAttempts) {
                    attempts++;
                    setTimeout(checkMonaco, 100);
                } else {
                    console.error('Monaco Editor failed to load');
                    reject(new Error('Monaco Editor not available'));
                }
            };

            checkMonaco();
        });
    },

    /**
     * Vloží text na pozici kurzoru
     */
    insertTextAtCursor(editor, text) {
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
     * Získá aktuální hodnotu editoru
     */
    getValue(editor) {
        return editor ? editor.getValue() : '';
    },

    /**
     * Nastaví hodnotu editoru
     */
    setValue(editor, value) {
        if (editor) {
            editor.setValue(value || '');
        }
    },

    /**
     * Zaměří editor
     */
    focus(editor) {
        if (editor) {
            editor.focus();
        }
    },

    /**
     * Získá pozici kurzoru
     */
    getCursorPosition(editor) {
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
    setCursorPosition(editor, lineNumber, column) {
        if (editor) {
            editor.setPosition({ lineNumber, column });
            editor.focus();
        }
    },

    /**
     * Získá vybraný text
     */
    getSelectedText(editor) {
        if (!editor) return '';
        const selection = editor.getSelection();
        return editor.getModel().getValueInRange(selection);
    }
};

// Auto-inicializace při načtení
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        console.log('Monaco loader ready');
    });
} else {
    console.log('Monaco loader ready');
}
