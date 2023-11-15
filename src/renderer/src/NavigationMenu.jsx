import React, { useState } from 'react';
import './NavigationMenu.css';



function NavItem({content, onClick}){
    return <li className='nav-item'>
            <a href='#' className='nav-link' onClick={onClick}>
                {content}
            </a>
        </li>;
}

export default function NavigationMenu()
{
    return <div className='navbar'>
        <ul className='navbar-nav'>
            <NavItem 
                content={<img src='hextale-icon.png' className='logo'></img>} 
                onClick={() => app.misc.openSite("https://hextale.xyz")}
            />
            <NavItem 
                content={<i className='icon-cog icon'></i>} 
                onClick={() => alert()}
            />
        </ul>
    </div>;
}