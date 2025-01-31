import React from 'react'
import {Routes, Route, Navigate} from 'react-router-dom'

import Home from '../pages/Home'
import Events from '../pages/Events'
import Residents from '../pages/Residents'
import ResidentApartments from '../pages/ResidentApartments'
import Apartments from '../pages/Apartments'
import EditResident from '../pages/EditResident'
import EditApartment from '../pages/EditApartment'
import EditEvent from '../pages/EditEvent'
import EditResidentApartment from '../pages/EditResidentApartment'

const Router = () => {
  return (
    <Routes>
        <Route path='/' element={<Navigate to = '/home' />} />
        <Route path='/home' element={<Home />} />
        <Route path='/events' element={<Events />} />
        <Route path='/edit-event/:id' element={<EditEvent />} />
        <Route path='/residents' element={<Residents />} />
        <Route path='/edit-resident/:id' element={<EditResident />} />
        <Route path='/apartments' element={<Apartments />} />
        <Route path='/edit-apartment/:id' element={<EditApartment />} />
        <Route path='/residentapartments' element={<ResidentApartments />} />
        <Route path='/edit-resident-apartment/:id' element={<EditResidentApartment />} />
    </Routes>
  )
}

export default Router