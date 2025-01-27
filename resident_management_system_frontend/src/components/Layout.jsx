import React from 'react'

import Header from '../components/Header';
import Router from '../router/Router';
import Footer from '../components/Footer';

import "../styles/Layout.css"

const Layout = () => {
  return (
    <>
    <Header />
    <div className="main-content">
      <Router />
    </div>
    <Footer />
    </>
  )
};

export default Layout;