﻿// see https://github.com/markvanwijnen/ServiceWorkerUpdateListener
class ServiceWorkerUpdateListener extends EventTarget { addRegistration(t) { if (this._registrations || (this._registrations = []), !this._registrations.includes(t)) { this._registrations.push(t); var e = (t, e, i, s) => { this._eventListeners || (this._eventListeners = []), this._eventListeners.push({ registration: t, target: e, type: i, listener: s }), e.addEventListener(i, s) }, i = (t, e, i) => { var s = "update" + t, n = "on" + s, r = new CustomEvent(s, { detail: { serviceWorker: e, registration: i } }); this.dispatchEvent(r), this[n] && "function" == typeof this[n] && this[n].call(this, r) }; t.waiting && i("waiting", t.waiting, t), e(t, t, "updatefound", s => { t.active && t.installing && (e(t, t.installing, "statechange", e => { "installed" === e.target.state && i("waiting", t.waiting, t) }), i("installing", t.installing, t)) }), e(t, navigator.serviceWorker, "controllerchange", t => { t.target.ready.then(t => { i("ready", t.active, t) }) }) } } removeRegistration(t) { if (this._registrations && !(this._registrations.length <= 0)) { var e = t => { this._eventListeners || (this._eventListeners = []), this._eventListeners = this._eventListeners.filter(e => e.registration !== t || (e.target.removeEventListener(e.type, e.listener), !1)) }; this._registrations = this._registrations.filter(i => i !== t || (e(t), !1)) } } skipWaiting(t) { t.postMessage("skipWaiting") } }

var listener = new ServiceWorkerUpdateListener();

navigator.serviceWorker.register('/service-worker.js', { updateViaCache: 'none' })
    .then(registration => {
        listener.addRegistration(registration)
        setInterval(() => {
            registration.update();
        }, 30 * 1000); // check every 30 seconds for an update
    });

listener.onupdateready = _ => window.location.reload();

// methods called from the UpdateAvailableButton component
var waitingServiceWorker;

window.subscribeToUpdateAvailable = (caller, methodName) => {
    // check if there is already an update available
    var registration = navigator.serviceWorker.getRegistration();
    if (registration) {
        registration.then(registration => {
            if (registration.waiting) {
                waitingServiceWorker = registration.waiting;
                caller.invokeMethodAsync(methodName);
            }
        });
    }

    // subscribe to the update available event
    listener.onupdatewaiting = event => {
        waitingServiceWorker = event.detail.serviceWorker;
        caller.invokeMethodAsync(methodName);
    };
}

window.subscribeToInstallUpdate = (caller, methodName) =>
    navigator.serviceWorker.addEventListener('message', event => {
        if (event.data === 'installUpdate') {
            caller.invokeMethodAsync(methodName)
        }
    });

window.installUpdateOnAllClients = () => {
    if (navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage('installUpdateOnAllClients');
        return true;
    }
    return false;
}

window.installUpdate = () =>
    listener.skipWaiting(waitingServiceWorker);