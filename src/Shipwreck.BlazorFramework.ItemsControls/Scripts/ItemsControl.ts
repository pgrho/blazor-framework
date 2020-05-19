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

    export function scrollTo(element: HTMLElement, left: number, top: number, smooth: boolean) {
        if (element) {
            element.scrollTo({
                left,
                top,
                behavior: smooth ? 'smooth' : 'auto'
            });
        }
    }

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
                try {
                    await obj.invokeMethodAsync('OnElementScroll', JSON.stringify(__itemsControlInfo(element, itemSelector)));
                } catch (ex) {
                    console.log(ex);
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
    export function scrollToItem(element: HTMLElement, itemSelector: string, index: number, localY: number, column: number, smooth: boolean) {
        if (element) {
            const items = element.querySelectorAll(itemSelector);

            for (let i = 0; i < items.length; i++) {
                const e = items[i] as HTMLElement;

                const sf = parseInt(e.getAttribute('data-itemindex'), 10);

                if (sf <= index) {
                    const la = e.getAttribute('data-itemlastindex');

                    if (la) {
                        const sl = parseInt(la, 10);

                        if (index <= sl) {
                            const b = __offsetInfo(e);
                            const nst = b.Top + Math.floor((index - sf) / column) * b.Height + localY;

                            if (Math.abs(nst - element.scrollTop) > 16) {

                                element.scrollTo({
                                    left: 0,
                                    top: nst,
                                    behavior: smooth ? 'smooth' : 'auto'
                                });
                            }

                            return;
                        }
                    } else if (sf === index) {
                        const b = __offsetInfo(e);

                        const nst = b.Top + localY;

                        if (Math.abs(nst - element.scrollTop) * 2 > b.Height) {

                            element.scrollTo({
                                left: 0,
                                top: nst,
                                behavior: smooth ? 'smooth' : 'auto'
                            });
                        }

                        return;
                    }
                }
            }
        }
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
        const elements: IElement[] = [];

        for (let i = 0; i < items.length; i++) {
            const e = items[i] as HTMLElement;

            const b: IElement = __offsetInfo(e);
            const bottom = b.Top + b.Height;
            if (vt <= bottom
                && e.offsetTop <= vb) {
                const sf = parseInt(e.getAttribute('data-itemindex'), 10);
                if (sf >= 0) {
                    const la = e.getAttribute('data-itemlastindex');

                    const sl = la ? parseInt(la, 10) : sf;

                    b.FirstIndex = sf;
                    b.LastIndex = sl;

                    if (!min || min.FirstIndex > sf) {
                        min = b;
                    }
                    if (!max || max.LastIndex < sl) {
                        max = b;
                    }

                    if (!la) {
                        minWidth = Math.min(minWidth, b.Width);
                        minHeight = Math.min(minHeight, b.Height);
                    }

                    if (elements.length) {
                        const prev = elements[elements.length - 1];
                        if (prev.Top >= b.Top) {
                            prev.LastIndex = b.LastIndex;
                            prev.Height = Math.max(prev.Height, b.Top + b.Height - prev.Top);
                            continue;
                        }
                    }
                    elements.push(<any>{
                        FirstIndex: b.FirstIndex,
                        LastIndex: b.LastIndex,
                        Top: b.Top,
                        Height: b.Height
                    });
                }
            }
        }

        return {
            Viewport: si,
            First: min,
            Last: max,
            MinWidth: minWidth !== Number.MAX_SAFE_INTEGER ? minWidth : 0,
            MinHeight: minHeight !== Number.MAX_SAFE_INTEGER ? minHeight : 0,
            Lines: elements
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

    function __parsePx(s: string): number {
        return s !== null && s !== undefined && /^([\-+]?\d+(?:\.\d+)?)px$/.test(s) ? parseFloat(RegExp.$1) : 0;
    }

    function __offsetInfo(element: HTMLElement): IBound {
        const s = window.getComputedStyle(element);
        const ml = __parsePx(s.marginLeft);
        const mt = __parsePx(s.marginTop);
        //const mr = __parsePx(s.marginRight);
        //const mb = __parsePx(s.marginBottom);

        return ({
            Left: element.offsetLeft - ml,
            Top: element.offsetTop - mt,
            Width: element.offsetWidth + ml,
            Height: element.offsetHeight + mt
        });
    }
}