using System;
using System.Diagnostics;

namespace GraphFramework {
    public static class EventHandlerExt {
        /// <summary>
        /// Max. milliseconds an eventhandler may take before it is logged.
        /// If set to int.Maxvalue, the eventhandlers are not monitored.
        /// </summary>
        public static int MaxMillisecondsBeforeReportingHandler = int.MaxValue; // Disabled

        [DebuggerStepThrough]
        private static bool ReportingHandlerEnabled() {
            return MaxMillisecondsBeforeReportingHandler != int.MaxValue;
        }

        [DebuggerStepThrough]
        private static void ExecuteAndMonitor(string callbackName, Action action) {
            DateTime started = DateTime.Now;
            try {
                action.Invoke();
            } finally {
                //int elapsed = (int)(DateTime.Now - started).TotalMilliseconds;
                //if (elapsed >= MaxMillisecondsBeforeReportingHandler) {
                //    DebugLog.Log("Raise: {0} took {1}ms", callbackName, elapsed);
                //}
            }
        }

        [DebuggerStepThrough]
        public static void Raise(this EventHandler handler, object sender) {
            if (handler != null) {
                handler.Raise(sender, EventArgs.Empty);
            }
        }

        [DebuggerStepThrough]
        public static void Raise(this EventHandler handler, string eventName, object sender) {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    Raise(handler, eventName, sender, EventArgs.Empty);
                } else {
                    handler(sender, EventArgs.Empty);
                }
            }
        }

        [DebuggerStepThrough]
        public static void Raise(this EventHandler handler, object sender, EventArgs e) {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    Raise(handler, sender.GetType() + "/" + handler, sender, e);
                } else {
                    handler(sender, e);
                }
            }
        }

        [DebuggerStepThrough]
        public static void Raise(this EventHandler handler, string eventName, object sender, EventArgs e) {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    ExecuteAndMonitor(sender.GetType() + "." + eventName, () => handler(sender, e));
                } else {
                    handler(sender, e);
                }
            }
        }

        [DebuggerStepThrough]
        public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    ExecuteAndMonitor(sender.GetType() + "/" + handler, () => handler(sender, e));
                } else {
                    handler(sender, e);
                }
            }
        }

        [DebuggerStepThrough]
        public static void Raise<T>(this EventHandler<T> handler, string eventName, object sender, T e) where T : EventArgs {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    ExecuteAndMonitor(sender.GetType() + "." + eventName, () => handler(sender, e));
                } else {
                    handler(sender, e);
                }
            }
        }

        [DebuggerStepThrough]
        public static void Raise<T>(this EventHandler<EventArg<T>> handler, string eventName, object sender, T e) {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    ExecuteAndMonitor(sender.GetType() + "." + eventName, () => handler(sender, new EventArg<T>(e)));
                } else {
                    handler(sender, new EventArg<T>(e));
                }
            }
        }

        [DebuggerStepThrough]
        public static void Raise<T>(this EventHandler<EventArg<T>> handler, object sender, T e) {
            if (handler != null) {
                if (ReportingHandlerEnabled()) {
                    ExecuteAndMonitor(sender.GetType() + "." + handler, () => handler(sender, new EventArg<T>(e)));
                } else {
                    handler(sender, new EventArg<T>(e));
                }
            }
        }
    }
}
