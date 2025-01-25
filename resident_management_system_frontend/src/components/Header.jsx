import React, {useRef, useEffect} from "react";
import { NavLink, Link, useNavigate } from "react-router-dom";
import "../styles/Header.css";

const navigacijski_linkovi=[
  {
    path: '/home',
    display : 'Home'
  },
  {
    path: '/residents',
    display : 'Residents'
  },
  {
    path: '/events',
    display : 'Events'
  },
  {
    path: '/apartments',
    display : 'Apartments'
  },
  {
    path: '/residentapartments',
    display : 'Residents & Apartments'
  }
]

const Header = () => {

  const headerRef = useRef(null)

  const stickyHeaderFunc = () => {
    window.addEventListener('scroll', () => {
      if(document.body.scrollTop > 80 || document.documentElement.scrollTop > 80){
        headerRef.current.classList.add('sticky__header')
      }else{
        headerRef.current.classList.remove('sticky__header')
      }
    })
  }

  useEffect(() => {
    stickyHeaderFunc()

    return window.removeEventListener('scroll', stickyHeaderFunc)
  })

  return (
    <header className="header" ref={headerRef}>
      <nav>
      
        <div className="navigation">
              <ul className="menu d-flex align-items-center gap-5">
                {navigacijski_linkovi.map((putanja, index) => (
                  <li className='nav__item' key={index}>
                    <NavLink to={putanja.path} className={navClass => navClass.isActive ? 'active__link' : ''
                  }>
                      {putanja.display}</NavLink>
                  </li>
                ))}
              </ul>
          </div>
        
      </nav>
    </header>
  );
}

export default Header;
