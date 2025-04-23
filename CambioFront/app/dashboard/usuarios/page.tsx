"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Button } from "@/components/ui/button"
import { useToast } from "@/components/ui/use-toast"
import { useAuth } from "@/contexts/auth-context"

interface User {
  Id: number
  Name: string
  Email: string
  IsMaster: boolean
}

export default function UsersPage() {
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(false)
  const { toast } = useToast()
  const { user: currentUser } = useAuth()

  
  const fetchUsers = async () => {
    setLoading(true)
    try {
      const response = await api.get("/api/Auth/all-users")
      
      const filteredUsers = response.data
        .filter((u: User) => u.Id !== currentUser?.Id)
        .sort((a: User, b: User) => a.Name.localeCompare(b.Name))
      setUsers(filteredUsers)
    } catch (error) {
      console.error("Erro ao buscar usuários:", error)
      toast({
        title: "Erro ao carregar usuários",
        description: "Não foi possível carregar a lista de usuários.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  
  const updateUserRole = async (UserId: number, IsMaster: boolean) => {
    setLoading(true)
    try {
      await api.post("/api/Auth/update-role", {
        UserId: UserId,
        IsMaster: IsMaster,
      })
      toast({
        title: "Sucesso",
        description: `Usuário ${IsMaster ? "tornado Master" : "removido de Master"} com sucesso.`,
      })
      fetchUsers() 
    } catch (error) {
      console.error("Erro ao atualizar papel do usuário:", error)
      toast({
        title: "Erro ao atualizar papel",
        description: "Não foi possível atualizar o papel do usuário.",
        variant: "destructive",
      })
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchUsers()
  }, [])

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold">Gerenciar Usuários</h1>
      {loading && <p>Carregando...</p>}
      {!loading && users.length === 0 && <p>Nenhum usuário encontrado.</p>}
      <ul className="space-y-4">
        {users.map((user) => (
          <li key={user.Id} className="flex items-center justify-between border p-4 rounded">
            <div>
              <p className="font-medium">{user.Name}</p>
              <p className="text-sm text-muted-foreground">{user.Email}</p>
              {user.IsMaster && <p className="text-xs font-medium text-primary">Usuário Master</p>}
            </div>
            <div className="flex gap-2">
              {user.IsMaster ? (
                <Button
                  variant="destructive"
                  onClick={() => updateUserRole(user.Id, false)}
                  disabled={loading}
                >
                  Remover Master
                </Button>
              ) : (
                <Button
                  variant="default"
                  onClick={() => updateUserRole(user.Id, true)}
                  disabled={loading}
                >
                  Tornar Master
                </Button>
              )}
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}