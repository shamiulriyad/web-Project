import { useEffect, useMemo, useState } from 'react'
import {
  createAddress,
  deleteAddress,
  getAddresses,
  getCurrentUserId,
  updateAddress,
  type AddressView,
} from '../services/marketplace'

type AddressForm = {
  name: string
  phone: string
  addressLine: string
  city: string
  postalCode: string
  isDefault: boolean
}

export default function AddressManagement() {
  const userId = useMemo(() => getCurrentUserId(), [])
  const [addresses, setAddresses] = useState<AddressView[]>([])
  const [editingId, setEditingId] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState<AddressForm>({
    name: '',
    phone: '',
    addressLine: '',
    city: '',
    postalCode: '',
    isDefault: false,
  })

  const loadAddresses = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await getAddresses(userId)
      setAddresses(data)
    } catch {
      setError('Failed to load addresses. Please login and try again.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadAddresses()
  }, [])

  const resetForm = () => {
    setForm({ name: '', phone: '', addressLine: '', city: '', postalCode: '', isDefault: false })
    setEditingId(null)
  }

  const submit = async () => {
    if (!form.name || !form.phone || !form.addressLine || !form.city) {
      setError('Please fill all required fields.')
      return
    }

    try {
      setError(null)
      if (editingId) {
        await updateAddress(editingId, {
          userId,
          name: form.name,
          phone: form.phone,
          addressLine: form.addressLine,
          city: form.city,
          postalCode: form.postalCode || undefined,
          isDefault: form.isDefault,
        })
      } else {
        await createAddress({
          userId,
          name: form.name,
          phone: form.phone,
          addressLine: form.addressLine,
          city: form.city,
          postalCode: form.postalCode || undefined,
          isDefault: form.isDefault,
        })
      }

      resetForm()
      await loadAddresses()
    } catch {
      setError('Failed to save address.')
    }
  }

  const startEdit = (address: AddressView) => {
    setEditingId(address.id)
    setForm({
      name: address.name,
      phone: address.phone,
      addressLine: address.addressLine,
      city: address.city,
      postalCode: address.postalCode ?? '',
      isDefault: address.isDefault,
    })
  }

  const remove = async (id: string) => {
    try {
      await deleteAddress(id, userId)
      await loadAddresses()
    } catch {
      setError('Failed to delete address.')
    }
  }

  return (
    <section className="max-w-5xl mx-auto px-6 py-10">
      <h1 className="text-2xl font-semibold text-dibi-900 mb-6">Address Management</h1>

      {error && <p className="text-sm text-red-600 mb-4">{error}</p>}

      <div className="grid md:grid-cols-2 gap-6">
        <div className="bg-white rounded-xl shadow-soft p-5">
          <h2 className="text-lg font-semibold mb-4">{editingId ? 'Edit Address' : 'Add New Address'}</h2>
          <div className="space-y-3">
            <input value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} placeholder="Full Name" className="w-full border rounded px-3 py-2" />
            <input value={form.phone} onChange={(e) => setForm((f) => ({ ...f, phone: e.target.value }))} placeholder="Phone" className="w-full border rounded px-3 py-2" />
            <input value={form.addressLine} onChange={(e) => setForm((f) => ({ ...f, addressLine: e.target.value }))} placeholder="Address Line" className="w-full border rounded px-3 py-2" />
            <input value={form.city} onChange={(e) => setForm((f) => ({ ...f, city: e.target.value }))} placeholder="City" className="w-full border rounded px-3 py-2" />
            <input value={form.postalCode} onChange={(e) => setForm((f) => ({ ...f, postalCode: e.target.value }))} placeholder="Postal Code" className="w-full border rounded px-3 py-2" />
            <label className="inline-flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={form.isDefault}
                onChange={(e) => setForm((f) => ({ ...f, isDefault: e.target.checked }))}
              />
              Set as default address
            </label>
            <div className="flex gap-2 pt-1">
              <button onClick={submit} className="px-4 py-2 rounded bg-dibi-500 text-white">{editingId ? 'Update' : 'Save'}</button>
              {editingId && (
                <button onClick={resetForm} className="px-4 py-2 rounded border">Cancel</button>
              )}
            </div>
          </div>
        </div>

        <div className="bg-white rounded-xl shadow-soft p-5">
          <h2 className="text-lg font-semibold mb-4">Saved Addresses</h2>
          {loading && <p className="text-sm text-neutral-600">Loading...</p>}
          {!loading && addresses.length === 0 && <p className="text-sm text-neutral-600">No addresses found.</p>}
          <div className="space-y-3">
            {addresses.map((address) => (
              <div key={address.id} className="border rounded p-3">
                <p className="font-medium text-dibi-900">{address.name} {address.isDefault && <span className="text-xs text-dibi-500">(Default)</span>}</p>
                <p className="text-sm text-neutral-700">{address.phone}</p>
                <p className="text-sm text-neutral-700">{address.addressLine}</p>
                <p className="text-sm text-neutral-700">{address.city}{address.postalCode ? `, ${address.postalCode}` : ''}</p>
                <div className="mt-2 flex gap-3 text-sm">
                  <button onClick={() => startEdit(address)} className="text-dibi-500">Edit</button>
                  <button onClick={() => remove(address.id)} className="text-red-600">Delete</button>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
