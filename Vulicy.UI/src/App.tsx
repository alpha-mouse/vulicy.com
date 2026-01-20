import Map from './components/Map'
import { ConfigProvider } from './hooks/useConfig'

function App() {
  return (
    <ConfigProvider>
      <div className="w-full h-full relative">
        <Map />
      </div>
    </ConfigProvider>
  )
}

export default App
