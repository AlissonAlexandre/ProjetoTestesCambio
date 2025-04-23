"use client"

import type React from "react"

import { useState } from "react"
import Link from "next/link"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet"
import { useAuth } from "@/contexts/auth-context"
import { LayoutDashboard, DollarSign, Users, FileText, Settings, Menu, ShieldCheck } from "lucide-react"

interface SidebarProps extends React.HTMLAttributes<HTMLDivElement> {}

export function Sidebar({ className }: SidebarProps) {
  const pathname = usePathname()
  const [open, setOpen] = useState(false)
  const { user } = useAuth()

  const routes = [
    {
      label: "Dashboard",
      icon: LayoutDashboard,
      href: "/dashboard",
      active: pathname === "/dashboard",
    },
    {
      label: "Cotação",
      icon: DollarSign,
      href: "/dashboard/cotacao",
      active: pathname === "/dashboard/cotacao",
    },
    {
      label: "Clientes",
      icon: Users,
      href: "/dashboard/clientes",
      active: pathname.includes("/dashboard/clientes"),
    },
    {
      label: "Operações",
      icon: FileText,
      href: "/dashboard/operacoes",
      active: pathname.includes("/dashboard/operacoes"),
    },
    {
      label: "Usuários",
      icon: ShieldCheck,
      href: "/dashboard/usuarios",
      active: pathname === "/dashboard/usuarios",
      IsMasterOnly: true,
    },
    {
      label: "Configurações",
      icon: Settings,
      href: "/dashboard/configuracoes",
      active: pathname === "/dashboard/configuracoes",
    },
  ]

  return (
    <>
      <Sheet open={open} onOpenChange={setOpen}>
        <SheetTrigger asChild className="lg:hidden">
          <Button variant="outline" size="icon" className="ml-2 mt-2">
            <Menu className="h-5 w-5" />
            <span className="sr-only">Toggle Menu</span>
          </Button>
        </SheetTrigger>
        <SheetContent side="left" className="w-[240px] p-0">
          <MobileSidebar routes={routes} setOpen={setOpen} />
        </SheetContent>
      </Sheet>
      <aside className={cn("hidden lg:flex lg:flex-col h-full w-[240px] border-r bg-background", className)}>
        <DesktopSidebar routes={routes} />
      </aside>
    </>
  )
}

interface SidebarRouteProps {
  routes: {
    label: string
    icon: any
    href: string
    active: boolean
    IsMasterOnly?: boolean
  }[]
}

function DesktopSidebar({ routes }: SidebarRouteProps) {
  const { user } = useAuth()

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-14 items-center border-b px-4">
        <Link href="/dashboard" className="flex items-center gap-2 font-semibold">
          <DollarSign className="h-6 w-6 text-primary" />
          <span>Sistema de Câmbio</span>
        </Link>
      </div>
      <ScrollArea className="flex-1 py-4">
        <nav className="grid gap-1 px-2">
          {routes.map((route) => {
            if (route.IsMasterOnly && (!user || !user.IsMaster)) {
              return null
            }

            return (
              <Link
                key={route.href}
                href={route.href}
                className={cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  route.active ? "bg-primary text-primary-foreground" : "hover:bg-muted",
                )}
              >
                <route.icon className="h-5 w-5" />
                {route.label}
              </Link>
            )
          })}
        </nav>
      </ScrollArea>
    </div>
  )
}

interface MobileSidebarProps extends SidebarRouteProps {
  setOpen: (open: boolean) => void
}

function MobileSidebar({ routes, setOpen }: MobileSidebarProps) {
  const { user } = useAuth()

  return (
    <div className="flex h-full flex-col">
      <div className="flex h-14 items-center border-b px-4">
        <Link href="/dashboard" className="flex items-center gap-2 font-semibold" onClick={() => setOpen(false)}>
          <DollarSign className="h-6 w-6 text-primary" />
          <span>Sistema de Câmbio</span>
        </Link>
      </div>
      <ScrollArea className="flex-1 py-4">
        <nav className="grid gap-1 px-2">
          {routes.map((route) => {
            if (route.IsMasterOnly && (!user || !user.IsMaster)) {
              return null
            }

            return (
              <Link
                key={route.href}
                href={route.href}
                onClick={() => setOpen(false)}
                className={cn(
                  "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                  route.active ? "bg-primary text-primary-foreground" : "hover:bg-muted",
                )}
              >
                <route.icon className="h-5 w-5" />
                {route.label}
              </Link>
            )
          })}
        </nav>
      </ScrollArea>
    </div>
  )
}
