import {AccountInfo} from '@azure/msal-browser';
import axios from 'axios';
import MockAdapter from 'axios-mock-adapter';

import {
  apiRouteAdminUserProfileGetAllUsersRoles,
  apiRouteAdminUserProfileGetUserByID,
  apiRouteAdminUserProfileUpdateUserRoles,
} from '../routes';

import {getAllUsersRoles, getUserById, updateUserRoles} from './admin';

const mock = new MockAdapter(axios);

jest.mock('../helpers', () => {
  return {
    getAccessToken: jest.fn(() => {
      return 'access_token';
    }),
    getGraphAccessToken: jest.fn(() => {
      return 'graph_access_token';
    }),
    getUserId: jest.fn(() => {
      return 'homeAccountId';
    }),
  };
});

it('getAllUsersRoles returns success', async () => {
  const data: any = [];

  const url = `${apiRouteAdminUserProfileGetAllUsersRoles}`;

  mock.onGet(url).reply(200, data);

  const res = await getAllUsersRoles();
  expect(res).toEqual(data);
});

it('getUserById returns success', async () => {
  const data: any = [];
  const accounts: AccountInfo[] = [
    {
      environment: 'environment',
      homeAccountId: 'homeAccountId',
      localAccountId: 'localAccountId',
      tenantId: 'tenantId',
      username: 'username',
    },
  ];

  const url = `${apiRouteAdminUserProfileGetUserByID}?userId=${accounts[0].homeAccountId}`;

  mock.onGet(url).reply(200, data);

  const res = await getUserById(accounts[0].homeAccountId);
  expect(res).toEqual(data);
});

it('updateUserRoles returns success', async () => {
  const data = {response: true};
  const roles = ['Admin'];
  const accounts: AccountInfo[] = [
    {
      environment: 'environment',
      homeAccountId: 'homeAccountId',
      localAccountId: 'localAccountId',
      tenantId: 'tenantId',
      username: 'username',
    },
  ];
  const url = `${apiRouteAdminUserProfileUpdateUserRoles}?oId=${accounts[0].homeAccountId}`;
  mock.onPut(url).reply(200, data);

  const res = await updateUserRoles(roles, accounts[0].homeAccountId);
  expect(res).toEqual(data);
});
