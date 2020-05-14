/*!
 * Shipwreck.BlazorFramework.ItemsControls
 */
namespace Shipwreck.BlazorFramework.ItemsControls {
    interface DotNetObjectReference {
        invokeMethodAsync(methodName: string, ...args);
    }
    interface IHasWindowResize extends DotNetObjectReference {
        __ItemsControl__onResize: Function;
    }
    interface IHasElementScroll extends DotNetObjectReference {
        __ItemsControl__onScroll: Function;
    }
    const WR = '__ItemsControl__onResize';
    const ES = '__ItemsControl__onScroll';
    export function attachWindowResize(obj: IHasWindowResize) {
        if (obj && !obj[WR]) {
            const h = async () => {
                try {
                    await obj.invokeMethodAsync('OnWindowResized');
                } catch (ex) {
                    console.log(ex);
                }
            };
            obj[WR] = h;
            window.addEventListener('resize', h, { passive: true });
        }
    }
    export function detachWindowResize(obj: IHasWindowResize) {
        let h: any
        if (obj && (h = obj[WR])) {
            window.removeEventListener('resize', h);
            delete obj[WR];
        }
    }
    interface IBound {
        Left: number;
        Top: number;
        Width: number;
        Height: number;
    }
    interface IElement extends IBound {
        FirstIndex?: number;
        LastIndex?: number;
    }

    export function attachElementScroll(element: HTMLElement, obj: IHasElementScroll, itemSelector: string) {
        if (obj && !obj[ES]) {
            const h = async () => {
                const s = new Date().getTime();
                try {
                    await obj.invokeMethodAsync('OnElementScroll', JSON.stringify(__itemsControlInfo(element, itemSelector)));
                } catch (ex) {
                    console.log(ex);
                } finally {
                    console.log(`Processed onscroll in ${new Date().getTime() - s}ms`);
                }
            };
            obj[ES] = h;
            element.addEventListener('scroll', h, { passive: true });
        }
    }

    export function getScrollInfo(element: Element) {
        return JSON.stringify(__scrollInfo(element));
    }
    export function getItemsControlScrollInfo(element: HTMLElement, itemSelector: string) {
        return JSON.stringify(__itemsControlInfo(element, itemSelector));
    }
    function __itemsControlInfo(element: HTMLElement, itemSelector: string) {
        const si = __scrollInfo(element);
        const items = element.querySelectorAll(itemSelector);

        const vt = si.ScrollTop;
        const vb = vt + si.ClientHeight;

        let minWidth = Number.MAX_SAFE_INTEGER;
        let minHeight = Number.MAX_SAFE_INTEGER;

        let min: IElement;
        let max: IElement;

        for (let i = 0; i < items.length; i++) {
            const e = items[i] as HTMLElement;

            const b: IElement = __offsetInfo(e);
            const bottom = b.Top + b.Height;

            if (vt <= bottom
                && e.offsetTop <= vb) {
                const sf = parseInt(e.getAttribute('data-itemindex'), 10);
                if (sf >= 0) {
                    const sl = parseInt(e.getAttribute('data-itemlastindex'), 10) || sf;

                    b.FirstIndex = sf;
                    b.LastIndex = sl;

                    if (!min || min.FirstIndex > sf) {
                        min = b;
                    }
                    if (!max || max.LastIndex < sl) {
                        max = b;
                    }

                    if (sf === sl) {
                        minWidth = Math.min(minWidth, b.Width);
                        minHeight = Math.min(minHeight, b.Height);
                    }
                }
            }
        }

        return {
            Viewport: si,
            First: min,
            Last: max,
            MinWidth: minWidth !== Number.MAX_SAFE_INTEGER ? minWidth : 0,
            MinHeight: minHeight !== Number.MAX_SAFE_INTEGER ? minHeight : 0,
        };
    }

    function __scrollInfo(element: Element) {
        return ({
            ClientLeft: element.clientLeft,
            ClientTop: element.clientTop,
            ClientWidth: element.clientWidth,
            ClientHeight: element.clientHeight,
            ScrollLeft: element.scrollLeft,
            ScrollTop: element.scrollTop,
            ScrollWidth: element.scrollWidth,
            ScrollHeight: element.scrollHeight,
        });
    }

    function __offsetInfo(element: HTMLElement): IBound {
        return ({
            Left: element.offsetLeft,
            Top: element.offsetTop,
            Width: element.offsetWidth,
            Height: element.offsetHeight
        });
    }
}