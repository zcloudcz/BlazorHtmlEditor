/**
 * Design Canvas JavaScript Interop
 * Poskytuje základní interakci pro vlastní page builder
 */

window.DesignCanvasInterop = {
    dotnetRef: null,

    /**
     * Inicializace Design Canvas
     */
    initialize(dotnetReference) {
        console.log('DesignCanvasInterop: Initializing');
        this.dotnetRef = dotnetReference;

        // Setup drag & drop event listeners
        this.setupDragAndDrop();

        // Setup contenteditable placeholder handling
        this.setupEditablePlaceholders();

        console.log('DesignCanvasInterop: Initialized successfully');
    },

    /**
     * Setup placeholder handling pro contenteditable elementy
     */
    setupEditablePlaceholders() {
        document.addEventListener('focus', (e) => {
            if (e.target.hasAttribute('contenteditable') && e.target.getAttribute('contenteditable') === 'true') {
                // Pokud má placeholder text, vymaž ho při focusu
                const text = e.target.innerText || e.target.textContent || '';
                if (text.includes('Click to edit...')) {
                    e.target.innerHTML = '';
                }
            }
        }, true); // Use capture to catch before element's own handlers
    },

    /**
     * Setup HTML5 Drag & Drop
     */
    setupDragAndDrop() {
        // Event delegation pro všechny design-node elementy
        document.addEventListener('dragstart', (e) => {
            if (e.target.classList.contains('design-node') || e.target.closest('.design-node')) {
                const node = e.target.classList.contains('design-node')
                    ? e.target
                    : e.target.closest('.design-node');

                const nodeId = node.getAttribute('data-node-id');
                if (nodeId) {
                    e.dataTransfer.effectAllowed = 'move';
                    e.dataTransfer.setData('text/plain', nodeId);
                    node.classList.add('dragging');
                    console.log('Drag started:', nodeId);
                }
            }
        });

        document.addEventListener('dragend', (e) => {
            if (e.target.classList.contains('design-node') || e.target.closest('.design-node')) {
                const node = e.target.classList.contains('design-node')
                    ? e.target
                    : e.target.closest('.design-node');
                node.classList.remove('dragging');
                console.log('Drag ended');
            }
        });

        document.addEventListener('dragover', (e) => {
            e.preventDefault(); // Nutné pro povolení drop

            const target = e.target.closest('.design-node');
            if (target) {
                e.dataTransfer.dropEffect = 'move';
                target.classList.add('drag-over');
            }
        });

        document.addEventListener('dragleave', (e) => {
            const target = e.target.closest('.design-node');
            if (target) {
                target.classList.remove('drag-over');
            }
        });

        document.addEventListener('drop', (e) => {
            e.preventDefault();

            const target = e.target.closest('.design-node');
            if (target) {
                target.classList.remove('drag-over');

                const draggedNodeId = e.dataTransfer.getData('text/plain');
                const targetNodeId = target.getAttribute('data-node-id');

                if (draggedNodeId && targetNodeId && draggedNodeId !== targetNodeId) {
                    console.log(`Drop: ${draggedNodeId} -> ${targetNodeId}`);

                    // Zavolej Blazor callback
                    if (this.dotnetRef) {
                        this.dotnetRef.invokeMethodAsync('MoveNode', draggedNodeId, targetNodeId, -1);
                    }
                }
            }
        });
    },

    /**
     * Získá text z contenteditable elementu
     */
    getContentEditableText(selector) {
        if (typeof selector === 'string') {
            const element = document.querySelector(selector);
            return element ? (element.innerText || element.textContent || '') : '';
        } else if (selector && selector.innerText !== undefined) {
            // Už je to element
            return selector.innerText || selector.textContent || '';
        }
        return '';
    },

    /**
     * Získá text z elementu podle node ID
     */
    getTextByNodeId(nodeId) {
        // Nejdřív zkus najít .editable-content (starý způsob)
        let element = document.querySelector(`[data-node-id="${nodeId}"] .editable-content`);

        // Pokud není, zkus najít element s data-editable="true" (nový způsob)
        if (!element) {
            element = document.querySelector(`[data-node-id="${nodeId}"][data-editable="true"]`);
        }

        // Pokud stále není, zkus najít contenteditable element
        if (!element) {
            element = document.querySelector(`[data-node-id="${nodeId}"][contenteditable="true"]`);
        }

        return element ? (element.innerText || element.textContent || '') : '';
    },

    /**
     * Nastaví focus na element
     */
    focusElement(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.focus();
        }
    },

    /**
     * Cleanup
     */
    dispose() {
        console.log('DesignCanvasInterop: Disposing');
        this.dotnetRef = null;
        // Event listeners jsou na document, takže je nemusíme odstraňovat
    }
};

console.log('DesignCanvasInterop loaded');
