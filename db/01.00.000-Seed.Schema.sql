
SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_table_access_method = heap;

--
-- TOC entry 218 (class 1259 OID 33207)
-- Name: dm_collection; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dm_collection (
    id uuid NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL
);


--
-- TOC entry 217 (class 1259 OID 33202)
-- Name: dm_dataset; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dm_dataset (
    id uuid NOT NULL,
    code character varying(50) NOT NULL,
    name character varying(250) NOT NULL
);


--
-- TOC entry 219 (class 1259 OID 33212)
-- Name: dm_dataset_collection; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.dm_dataset_collection (
    id uuid NOT NULL,
    dataset_id uuid NOT NULL,
    collection_id uuid NOT NULL
);


--
-- TOC entry 222 (class 1259 OID 47214)
-- Name: user; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public."user" (
    id uuid NOT NULL,
    name character varying(250),
    email character varying(250),
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    idp_subject_id character varying(250) NOT NULL
);


--
-- TOC entry 220 (class 1259 OID 47204)
-- Name: user_collection; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_collection (
    id uuid NOT NULL,
    name character varying(250) NOT NULL,
    user_id uuid NOT NULL,
    is_active smallint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


--
-- TOC entry 221 (class 1259 OID 47209)
-- Name: user_dataset_collection; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.user_dataset_collection (
    id uuid NOT NULL,
    user_collection_id uuid NOT NULL,
    dataset_id uuid NOT NULL,
    is_active smallint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL
);


--
-- TOC entry 223 (class 1259 OID 47232)
-- Name: version_info; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.version_info (
    key character varying(30) NOT NULL,
    version character varying(50) NOT NULL,
    released_at timestamp with time zone NOT NULL,
    deployed_at timestamp with time zone NOT NULL,
    description text
);


--
-- TOC entry 3278 (class 2606 OID 33206)
-- Name: dm_dataset dataset_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dm_dataset
    ADD CONSTRAINT dataset_pkey PRIMARY KEY (id);


--
-- TOC entry 3280 (class 2606 OID 33211)
-- Name: dm_collection dm_collection_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dm_collection
    ADD CONSTRAINT dm_collection_pkey PRIMARY KEY (id);


--
-- TOC entry 3282 (class 2606 OID 33216)
-- Name: dm_dataset_collection dm_dataset_collection_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dm_dataset_collection
    ADD CONSTRAINT dm_dataset_collection_pkey PRIMARY KEY (id);


--
-- TOC entry 3284 (class 2606 OID 47208)
-- Name: user_collection user_collection_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_collection
    ADD CONSTRAINT user_collection_pkey PRIMARY KEY (id);


--
-- TOC entry 3286 (class 2606 OID 47213)
-- Name: user_dataset_collection user_dataset_collection_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_dataset_collection
    ADD CONSTRAINT user_dataset_collection_pkey PRIMARY KEY (id);


--
-- TOC entry 3288 (class 2606 OID 47227)
-- Name: user user_idp_subject_id_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_idp_subject_id_key UNIQUE (idp_subject_id);


--
-- TOC entry 3290 (class 2606 OID 47220)
-- Name: user user_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public."user"
    ADD CONSTRAINT user_pkey PRIMARY KEY (id);


--
-- TOC entry 3292 (class 2606 OID 47238)
-- Name: version_info version_info_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.version_info
    ADD CONSTRAINT version_info_pkey PRIMARY KEY (key);


--
-- TOC entry 3293 (class 2606 OID 33222)
-- Name: dm_dataset_collection dm_dataset_collection_collection_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dm_dataset_collection
    ADD CONSTRAINT dm_dataset_collection_collection_id_fkey FOREIGN KEY (collection_id) REFERENCES public.dm_collection(id) NOT VALID;


--
-- TOC entry 3294 (class 2606 OID 33217)
-- Name: dm_dataset_collection dm_dataset_collection_dataset_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.dm_dataset_collection
    ADD CONSTRAINT dm_dataset_collection_dataset_id_fkey FOREIGN KEY (dataset_id) REFERENCES public.dm_dataset(id) NOT VALID;


--
-- TOC entry 3295 (class 2606 OID 47221)
-- Name: user_collection user_collection_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.user_collection
    ADD CONSTRAINT user_collection_user_id_fkey FOREIGN KEY (user_id) REFERENCES public."user"(id) NOT VALID;


