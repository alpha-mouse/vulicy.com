import Map from './components/Map'
import { ConfigProvider } from './hooks/useConfig'
import * as Sentry from "@sentry/react";

function App() {
  return (
    <ConfigProvider>
      <div className="w-full h-full relative">
        <Sentry.ErrorBoundary fallback={<p>An error occurred</p>}>
          <Map />
        </Sentry.ErrorBoundary>
      </div>
    </ConfigProvider>
  )
}

export default App
