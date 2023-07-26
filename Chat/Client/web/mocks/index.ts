import { worker } from "./browser"

if (process.env.NODE_ENV === "mock") {
    worker.start()
}
