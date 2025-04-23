"use client"

import { createContext, useContext, useState, useEffect, type ReactNode } from "react"
import { api } from "@/lib/api"
import { useRouter } from "next/navigation"

interface User {
  Id: number
  Name: string
  Email: string
  IsMaster: boolean
}

interface AuthContextType {
  user: User | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  updateUser: (user: User) => void
  updateToken: (token: string) => void
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  loading: true,
  login: async () => {},
  logout: () => {},
  updateUser: () => {},
  updateToken: () => {}
})

export const useAuth = () => useContext(AuthContext)

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)
  const router = useRouter()

  useEffect(() => {
    const checkAuth = async () => {
      try {
        const token = localStorage.getItem("token")
        const storedUser = JSON.parse(localStorage.getItem("user") || "null")

        if (!token || !storedUser) {
          throw new Error("No token or user found")
        }

        api.defaults.headers.common["Authorization"] = `Bearer ${token}`

        const response = await api.get("/api/User/profile")
        if (response.data) {
          localStorage.setItem("user", JSON.stringify(response.data))
          setUser(response.data)
        } else {
          throw new Error("Invalid user data")
        }
      } catch (error) {
        localStorage.removeItem("token")
        localStorage.removeItem("user")
        delete api.defaults.headers.common["Authorization"]
        setUser(null)
        
        if (window.location.pathname !== "/login") {
          router.replace("/login")
        }
      } finally {
        setLoading(false)
      }
    }

    checkAuth()
  }, [])

  const login = async (email: string, password: string) => {
    try {
      const response = await api.post("/api/Auth/login", {
        email,
        password,
      })

      const { token, user } = response.data

      localStorage.setItem("token", token)
      localStorage.setItem("user", JSON.stringify(user))

      api.defaults.headers.common["Authorization"] = `Bearer ${token}`

      setUser(user)
      router.push("/dashboard")

      return user
    } catch (error) {
      throw error
    }
  }

  const logout = () => {
    localStorage.removeItem("token")
    localStorage.removeItem("user")
    delete api.defaults.headers.common["Authorization"]
    setUser(null)
    router.replace("/login")
  }

  const updateUser = (updatedUser: User) => {
    setUser(updatedUser)
    localStorage.setItem("user", JSON.stringify(updatedUser))
  }

  const updateToken = (token: string) => {
    localStorage.setItem("token", token)
    api.defaults.headers.common["Authorization"] = `Bearer ${token}`
  }

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, updateUser, updateToken }}>
      {children}
    </AuthContext.Provider>
  )
}
