import axios, { AxiosRequestHeaders } from "axios"

export const api = axios.create({
  baseURL: "https://localhost:7207",
  headers: {
    "Content-Type": "application/json",
  },
})


api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token")
  console.log("Token recuperado:", token) 
  if (token) {
    config.headers = config.headers || {} as AxiosRequestHeaders
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
}, (error) => {
  return Promise.reject(error)
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 302 || error.response?.status === 301) {
      const redirectUrl = error.response.headers.location
      if (redirectUrl) {
        const token = localStorage.getItem("token")
        return api.request({
          ...error.config,
          url: redirectUrl,
          headers: {
            ...error.config.headers,
            Authorization: `Bearer ${token}`,
          },
        })
      }
    }
    return Promise.reject(error)
  }
)



api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      try {
        
        const noRetryRoutes = ["/api/Auth/refresh-token", "/api/Auth/login"]
        if (noRetryRoutes.some(route => error.config.url?.includes(route))) {
          throw new Error("No retry for this route")
        }

        
        const { data } = await api.post("/api/Auth/refresh-token", {
          refreshToken: localStorage.getItem("token")
        })

        
        localStorage.setItem("token", data.token)

        
        error.config.headers = error.config.headers || {} as AxiosRequestHeaders
        error.config.headers.Authorization = `Bearer ${data.token}`
        error.config.headers.common = error.config.headers.common || {}
        error.config.headers.common.Authorization = `Bearer ${data.token}`
        
        return api(error.config)
      } catch (refreshError) {
        
        localStorage.removeItem("token")
        localStorage.removeItem("user")
        window.location.href = "/login"
      }
    }
    return Promise.reject(error)
  }
)
