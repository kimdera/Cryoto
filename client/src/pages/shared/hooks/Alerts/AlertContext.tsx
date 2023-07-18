import {createContext, useContext, useMemo, useState} from 'react';

import {AlertType} from './AlertType';

function AlertProvider(props: {children: any}) {
  const [alertType, setAlertType] = useState(AlertType.NONE);
  const [alertText, setAlertText] = useState('');

  const success = (text: string) => {
    setAlertText(text);
    setAlertType(AlertType.SUCCESS);
  };

  const error = (text: string) => {
    setAlertText(text);
    setAlertType(AlertType.ERROR);
  };

  const info = (text: string) => {
    setAlertText(text);
    setAlertType(AlertType.INFO);
  };

  const warning = (text: string) => {
    setAlertText(text);
    setAlertType(AlertType.WARNING);
  };

  const clear = () => {
    setAlertText('');
    setAlertType(AlertType.NONE);
  };

  const values = useMemo(() => {
    return {
      success,
      error,
      info,
      warning,
      clear,
      alertType,
      alertText,
    };
  }, [alertType, alertText]);

  return (
    <AlertContext.Provider value={values}>
      {props.children}
    </AlertContext.Provider>
  );
}

const AlertContext = createContext({
  alertType: AlertType.NONE,
  alertText: '',
  success: (text: string) => {},
  error: (text: string) => {},
  info: (text: string) => {},
  warning: (text: string) => {},
  clear: () => {},
});

const useAlertContext = () => {
  const context = useContext(AlertContext);
  if (context === undefined) {
    throw new Error('useAlertContext must be used within a AlertProvider');
  }
  return context;
};

export {AlertProvider, useAlertContext, AlertContext};
