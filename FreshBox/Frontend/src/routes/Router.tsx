import type { ReactElement } from 'react'
// import { AuthContext } from '../context/AuthContext'
import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from '../context/useAuth'


// Public Pages
import LandingPage from '../pages/LandingPage'
import Home from '../pages/Home'
import Category from '../pages/Category'
import ProductDetails from '../pages/ProductDetails'
import Cart from '../pages/Cart'
import Checkout from '../pages/Checkout'
import PaymentSuccess from '../pages/PaymentSuccess'
import PaymentFailed from '../pages/PaymentFailed'
import Login from '../pages/Login'
import Register from '../pages/Register'
import ForgotPassword from '../pages/ForgotPassword'
import UserProfile from '../pages/UserProfile'
import EditProfile from '../pages/EditProfile'
import AddressManagement from '../pages/AddressManagement'
import OrderHistory from '../pages/OrderHistory'
import OrderDetails from '../pages/OrderDetails'
import Wishlist from '../pages/Wishlist'
import SearchResults from '../pages/SearchResults'
import About from '../pages/About'
import Contact from '../pages/Contact'
import Terms from '../pages/Terms'
import Privacy from '../pages/Privacy'
import NotFound from '../pages/NotFound'

// Admin Pages
import AdminLogin from '../pages/admin/AdminLogin'
import AdminDashboard from '../pages/admin/AdminDashboard'
import ManageCategories from '../pages/admin/ManageCategories'
import AddEditCategory from '../pages/admin/AddEditCategory'
import ManageProducts from '../pages/admin/ManageProducts'
import AddProduct from '../pages/admin/AddProduct'
import EditProduct from '../pages/admin/EditProduct'
import ManageOrders from '../pages/admin/ManageOrders'
import AdminOrderDetails from '../pages/admin/AdminOrderDetails'
import ManageUsers from '../pages/admin/ManageUsers'
import ManageDiscounts from '../pages/admin/ManageDiscounts'
import PaymentManagement from '../pages/admin/PaymentManagement'
import Reports from '../pages/admin/Reports'
import AdminProfile from '../pages/admin/AdminProfile'
import SiteSettings from '../pages/admin/SiteSettings'

// Layouts
import MainLayout from '../layouts/MainLayout'
import AdminLayout from '../layouts/AdminLayout'

export default function Router() {
  function RequireAuth({ children }: { children: ReactElement }) {
    const { isLoggedIn } = useAuth()
    if (!isLoggedIn) return <Navigate to="/login" replace />
    return children
  }

  return (
    <Routes>
      {/* Public routes use MainLayout with guest navbar */}
      <Route element={<MainLayout navVariant="guest" />}>
        <Route index element={<LandingPage />} />
        <Route path="shop" element={<Home />} />
        <Route path="categories/:slug" element={<Category />} />
        <Route path="product/:id" element={<ProductDetails />} />
        <Route path="cart" element={<Cart />} />
        <Route path="checkout" element={<Checkout />} />
        <Route path="checkout/success" element={<PaymentSuccess />} />
        <Route path="checkout/failed" element={<PaymentFailed />} />
        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
        <Route path="forgot-password" element={<ForgotPassword />} />
        <Route path="wishlist" element={<Wishlist />} />
        <Route path="search" element={<SearchResults />} />
        <Route path="about" element={<About />} />
        <Route path="contact" element={<Contact />} />
        <Route path="terms" element={<Terms />} />
        <Route path="privacy" element={<Privacy />} />
        <Route path="*" element={<NotFound />} />
      </Route>

      {/* Protected routes use MainLayout with user navbar and RequireAuth */}
      <Route
        element={
          <RequireAuth>
            <MainLayout navVariant="user" />
          </RequireAuth>
        }
      >
        <Route path="profile" element={<UserProfile />} />
        <Route path="profile/edit" element={<EditProfile />} />
        <Route path="profile/addresses" element={<AddressManagement />} />
        <Route path="orders" element={<OrderHistory />} />
        <Route path="orders/:id" element={<OrderDetails />} />
      </Route>

      {/* Admin routes */}
      <Route path="/admin/login" element={<AdminLogin />} />
      <Route path="/admin" element={<AdminLayout />}>
        <Route index element={<AdminDashboard />} />
        <Route path="categories" element={<ManageCategories />} />
        <Route path="categories/new" element={<AddEditCategory />} />
        <Route path="categories/:id/edit" element={<AddEditCategory />} />
        <Route path="products" element={<ManageProducts />} />
        <Route path="products/new" element={<AddProduct />} />
        <Route path="products/:id/edit" element={<EditProduct />} />
        <Route path="orders" element={<ManageOrders />} />
        <Route path="orders/:id" element={<AdminOrderDetails />} />
        <Route path="users" element={<ManageUsers />} />
        <Route path="discounts" element={<ManageDiscounts />} />
        <Route path="payments" element={<PaymentManagement />} />
        <Route path="reports" element={<Reports />} />
        <Route path="profile" element={<AdminProfile />} />
        <Route path="settings" element={<SiteSettings />} />
      </Route>
    </Routes>
  )
}