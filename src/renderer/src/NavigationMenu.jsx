import React, { useState } from 'react';
import './NavigationMenu.css';



function NavItem({content, onClick}){
    return <li className='nav-item'>
            <a href='#' className='nav-link' onClick={onClick}>
                <i className='icon-cog icon'></i>
            </a>
        </li>;
}

export default function NavigationMenu()
{
    return <div className='navbar'>
        <ul className='navbar-nav'>
            <NavItem onClick={() => alert()}/>
        </ul>
    </div>;
}