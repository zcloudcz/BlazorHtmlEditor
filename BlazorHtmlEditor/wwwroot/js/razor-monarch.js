/**
 * Monarch definice pro Razor syntax highlighting
 * Podporuje Razor výrazy (@Model, @if, @foreach, atd.)
 */
export const razorLanguageDefinition = {
    tokenizer: {
        root: [
            // Razor komentáře @* ... *@
            [/@\*/, 'comment.razor', '@razorComment'],

            // Razor direktivy @page, @model, @using, atd.
            [/@(page|model|using|inject|implements|inherits|attribute|namespace|functions|section|layout)\b/, 'keyword.directive.razor'],

            // Razor code bloky @{ ... }
            [/@\{/, 'delimiter.razor', '@razorCodeBlock'],

            // Razor výrazy s metodami @Model.Method()
            [/@[A-Za-z_]\w*(\.\w+)*(\([^)]*\))?/, 'variable.razor'],

            // Razor control flow
            [/@(if|else|foreach|for|while|switch|case|do|try|catch|finally|using|lock)\b/, 'keyword.control.razor'],

            // HTML tagy
            [/<\/?[a-zA-Z][\w-]*/, 'tag.html', '@htmlTag'],

            // HTML komentáře
            [/<!--/, 'comment.html', '@htmlComment'],

            // Běžný text
            [/[^<@]+/, 'text'],
        ],

        razorComment: [
            [/[^*]+/, 'comment.razor'],
            [/\*@/, 'comment.razor', '@pop'],
            [/[*]/, 'comment.razor']
        ],

        razorCodeBlock: [
            [/@\{/, 'delimiter.razor', '@push'],
            [/\}/, 'delimiter.razor', '@pop'],
            [/@[A-Za-z_]\w*/, 'variable.razor'],
            [/"([^"\\]|\\.)*"/, 'string'],
            [/'([^'\\]|\\.)*'/, 'string'],
            [/\b(var|string|int|bool|double|decimal|float|long|object|dynamic|if|else|foreach|for|while|return|new|class|public|private|protected|static|void|async|await)\b/, 'keyword'],
            [/[{}()\[\]]/, 'delimiter'],
            [/[;,.]/, 'delimiter'],
            [/\d+/, 'number'],
            [/[a-zA-Z_]\w*/, 'identifier'],
        ],

        htmlTag: [
            [/\/>/, 'tag.html', '@pop'],
            [/>/, 'tag.html', '@pop'],
            [/\s+/, ''],
            [/[\w-]+/, 'attribute.name.html'],
            [/=/, 'delimiter'],
            [/"[^"]*"/, 'attribute.value.html'],
            [/'[^']*'/, 'attribute.value.html'],
            // Razor výrazy uvnitř atributů
            [/@[A-Za-z_]\w*(\.\w+)*/, 'variable.razor'],
        ],

        htmlComment: [
            [/[^-]+/, 'comment.html'],
            [/-->/, 'comment.html', '@pop'],
            [/[-]/, 'comment.html']
        ]
    }
};

/**
 * Registrace Razor jazyka v Monaco Editoru
 */
export function registerRazorLanguage() {
    if (typeof monaco === 'undefined') {
        console.error('Monaco editor is not loaded');
        return;
    }

    // Registrace jazyka
    monaco.languages.register({ id: 'razor' });

    // Nastavení Monarch tokenizeru
    monaco.languages.setMonarchTokensProvider('razor', razorLanguageDefinition);

    // Nastavení konfigurace jazyka
    monaco.languages.setLanguageConfiguration('razor', {
        comments: {
            blockComment: ['@*', '*@'],
            lineComment: '//'
        },
        brackets: [
            ['{', '}'],
            ['[', ']'],
            ['(', ')'],
            ['<', '>']
        ],
        autoClosingPairs: [
            { open: '{', close: '}' },
            { open: '[', close: ']' },
            { open: '(', close: ')' },
            { open: '"', close: '"' },
            { open: "'", close: "'" },
            { open: '<', close: '>' }
        ],
        surroundingPairs: [
            { open: '{', close: '}' },
            { open: '[', close: ']' },
            { open: '(', close: ')' },
            { open: '"', close: '"' },
            { open: "'", close: "'" },
            { open: '<', close: '>' }
        ],
        folding: {
            markers: {
                start: /^\s*@\{/,
                end: /^\s*\}/
            }
        }
    });

    // IntelliSense - completion provider
    monaco.languages.registerCompletionItemProvider('razor', {
        provideCompletionItems: (model, position) => {
            const word = model.getWordUntilPosition(position);
            const range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn
            };

            const suggestions = [
                // Razor direktivy
                {
                    label: '@model',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: '@model ',
                    documentation: 'Definuje typ modelu pro view',
                    range: range
                },
                {
                    label: '@using',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: '@using ',
                    documentation: 'Importuje namespace',
                    range: range
                },
                {
                    label: '@inject',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: '@inject ',
                    documentation: 'Injektuje službu',
                    range: range
                },
                {
                    label: '@page',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: '@page "',
                    documentation: 'Definuje route pro stránku',
                    range: range
                },

                // Razor control flow
                {
                    label: '@if',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '@if (${1:condition})\n{\n\t$0\n}',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'If statement',
                    range: range
                },
                {
                    label: '@foreach',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '@foreach (var ${1:item} in ${2:collection})\n{\n\t$0\n}',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Foreach loop',
                    range: range
                },
                {
                    label: '@for',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '@for (int ${1:i} = 0; ${1:i} < ${2:length}; ${1:i}++)\n{\n\t$0\n}',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'For loop',
                    range: range
                },
                {
                    label: '@while',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '@while (${1:condition})\n{\n\t$0\n}',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'While loop',
                    range: range
                },
                {
                    label: '@switch',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '@switch (${1:expression})\n{\n\tcase ${2:value}:\n\t\t$0\n\t\tbreak;\n}',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Switch statement',
                    range: range
                },

                // Code bloky
                {
                    label: '@{ }',
                    kind: monaco.languages.CompletionItemKind.Snippet,
                    insertText: '@{\n\t$0\n}',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    documentation: 'Code block',
                    range: range
                },

                // Model reference
                {
                    label: '@Model',
                    kind: monaco.languages.CompletionItemKind.Variable,
                    insertText: '@Model.',
                    documentation: 'Reference to the view model',
                    range: range
                }
            ];

            return { suggestions: suggestions };
        }
    });

    console.log('Razor language registered successfully');
}
