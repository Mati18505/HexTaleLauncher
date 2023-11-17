import React, { useState, useEffect, useRef } from 'react';
import './NavigationMenu.css';
import Menu from './Menu/Menu';
import imgLogo from '@img/hextale-icon.png';

function NavItem({content, onClick}){
    return <li className='nav-item'>
            <a href='#' className='nav-link' onClick={onClick}>
                {content}
            </a>
        </li>;
}

export default function NavigationMenu()
{
    var [menu, SetMenu] = useState("");
    const wrapperRef = useRef(null);

    //check if clicked outside of Menu
    useEffect(() => {
        function handleClickOutside(event) {
          if (wrapperRef.current && !wrapperRef.current.contains(event.target))
            SetMenu("");
        }
        document.addEventListener("mousedown", handleClickOutside);
        return () => {
          document.removeEventListener("mousedown", handleClickOutside);
        };
    }, [wrapperRef]);

    function ChangeMenu(newMenu)
    {
        if(menu == newMenu) 
            SetMenu("");
        else 
            SetMenu(newMenu);
    }

    return <div ref={wrapperRef}>
    <div className='navbar'>
        <ul className='navbar-nav'>
            <NavItem 
                content={<img src={imgLogo} className='logo'></img>} 
                onClick={() => app.misc.openSite("https://hextale.xyz")}
            />
            <NavItem 
                content={<i className='icon-cog icon'></i>} 
                onClick={() => ChangeMenu("settings")}
            />
        </ul>
    </div>
    <Menu menu={menu} onHide={() => SetMenu("")}/>
    </div>;
}