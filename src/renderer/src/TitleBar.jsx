import React, { useState } from 'react';
import './TitleBar.css';

function Minimize()
{
    app.window.minimize();
}

function CloseApp() 
{
    app.window.close();
}

export default function TitleBar()
{
    var [menu, ShowMenu] = useState(false);

    return <div id="TitleBar">
        <div className="left">
            {/* <span className='navLink options' onClick={() => ShowMenu(!menu)}>&#x2630;</span> */}
            <img src='hextale-icon.png' className='logo'></img>
            <div className="titleParent"><span className="title">HexTale Launcher</span></div>
        </div>
        <div className="right">
            <span className='navLink' onClick={Minimize}>&minus;</span>
            <span className='navLink' onClick={CloseApp}>&#10006;</span>
        </div>
        
    </div>;
}