// © Microsoft Corporation. All rights reserved.
import React from 'react';


import Navbar from 'react-bootstrap/Navbar';

import { useIsAuthenticated } from '@azure/msal-react';
import { SignInButton } from './SignInButton';
import { SignOutButton } from './SignOutButton';


export interface HomeScreenProps {
  createThreadHandler(): void;
}

    export const PageLayout = (props: { children: any; }) => { 
       
    const isAuthenticated = useIsAuthenticated();
    return (
        <>
        <Navbar bg="primary" variant="dark">
          {props.children}
         {isAuthenticated ? <SignOutButton /> : <SignInButton />}
         
        </Navbar>
       
      </>
    );
};